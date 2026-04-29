param(
    [ValidateSet('Collect', 'Compare')]
    [string]$Action = 'Collect',

    [string]$WorkspaceRoot = ([System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))),

    [string]$GameSourceRoot,

    [string]$OutputRoot = ([System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\.cache\patch-baseline'))),

    [string]$Version,

    [string[]]$Modules = @('QuantumMaster', 'CombatMaster'),

    [string[]]$IncludeConfigKey,

    [switch]$CopyGameSource,

    [string]$BaseVersion,

    [string]$TargetVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:ModuleSettings = @{
    QuantumMaster = @{
        SourceRoot = 'src/QuantumMaster'
        MainFile = 'QuantumMaster.cs'
        ConfigManagerFile = 'ConfigManager.cs'
        ConfigManagerName = 'ConfigManager'
    }
    CombatMaster = @{
        SourceRoot = 'src/CombatMaster'
        MainFile = 'CombatMaster.cs'
        ConfigManagerFile = 'CombatConfigManager.cs'
        ConfigManagerName = 'CombatConfigManager'
    }
}

$script:IgnoredConfigFields = @(
    'LuckyLevel'
)

$script:IgnoredManagerMembers = @(
    'IsFeatureEnabled',
    'LoadAllConfigs',
    'GetFeatureLuckLevel',
    'GetConfigValue'
)

$script:PresetMethodMap = @{
    'PatchPresets.Extensions.CheckPercentProb' = @{ Display = 'CheckPercentProb'; Name = 'CheckPercentProb'; Kind = 'Extension' }
    'PatchPresets.Extensions.CheckProb' = @{ Display = 'CheckProb'; Name = 'CheckProb'; Kind = 'Extension' }
    'PatchPresets.Extensions.CheckProbability' = @{ Display = 'CheckProbability'; Name = 'CheckProbability'; Kind = 'Static' }
    'PatchPresets.Extensions.CalculateFormula0Arg' = @{ Display = 'Calculate'; Name = 'Calculate'; Kind = 'Extension' }
    'PatchPresets.InstanceMethods.Next2Args' = @{ Display = 'Next(int,int)'; Name = 'Next'; Kind = 'Instance' }
    'PatchPresets.InstanceMethods.Next1Arg' = @{ Display = 'Next(int)'; Name = 'Next'; Kind = 'Instance' }
}

$script:PatchFileResolutionCache = @{}

$script:ModuleSourceFilesCache = @{}

$script:PatchClassScopeCache = @{}

$script:PatchMetadataCache = @{}

function Write-Status {
    param([string]$Message)

    Write-Host "[PatchBaseline] $Message"
}

function Get-TextHash {
    param([string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Ensure-Directory {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }

    return $Path
}

function Read-TextFile {
    param([string]$Path)

    return [System.IO.File]::ReadAllText($Path)
}

function Read-TextLines {
    param([string]$Path)

    return [System.IO.File]::ReadAllLines($Path)
}

function Get-DirectoryBuildPropsValue {
    param(
        [string]$WorkspaceRootPath,
        [string]$PropertyName
    )

    $propsPath = Join-Path $WorkspaceRootPath 'Directory.Build.props'
    if (-not (Test-Path -LiteralPath $propsPath)) {
        return $null
    }

    try {
        [xml]$propsXml = Read-TextFile -Path $propsPath
    }
    catch {
        return $null
    }

    foreach ($propertyGroup in @($propsXml.Project.PropertyGroup)) {
        if ($null -eq $propertyGroup) {
            continue
        }

        $propertyNode = $propertyGroup.SelectSingleNode($PropertyName)
        if ($null -eq $propertyNode) {
            continue
        }

        $value = [string]$propertyNode.InnerText
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value.Trim()
        }
    }

    return $null
}

function Resolve-ConfiguredGameSourceRoot {
    param(
        [string]$WorkspaceRootPath,
        [string]$OverrideRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($OverrideRoot)) {
        return $OverrideRoot.Trim()
    }

    foreach ($propertyName in @('PatchBaselineGameSourceRoot', 'GameSourceRoot')) {
        $configuredValue = Get-DirectoryBuildPropsValue -WorkspaceRootPath $WorkspaceRootPath -PropertyName $propertyName
        if (-not [string]::IsNullOrWhiteSpace($configuredValue)) {
            return $configuredValue
        }
    }

    return 'D:\code\Data'
}

function Normalize-StringList {
    param([string[]]$Values)

    $items = New-Object System.Collections.Generic.List[string]
    foreach ($value in $Values) {
        if ([string]::IsNullOrWhiteSpace($value)) {
            continue
        }

        foreach ($segment in ($value -split ',')) {
            $trimmed = $segment.Trim()
            if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
                $items.Add($trimmed) | Out-Null
            }
        }
    }

    return @($items | Select-Object -Unique)
}

function Convert-ToSafeName {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return 'empty'
    }

    $safe = $Text -replace '[\\/:*?"<>|]', '_'
    $safe = $safe -replace '\s+', '_'
    return $safe.Trim('_')
}

function Get-RelativeWorkspacePath {
    param([string]$Path)

    if (-not $Path) {
        return $null
    }

    $workspaceUri = New-Object System.Uri((Ensure-TrailingSeparator -Path $WorkspaceRoot))
    $pathUri = New-Object System.Uri($Path)
    $relative = $workspaceUri.MakeRelativeUri($pathUri).ToString()
    return [System.Uri]::UnescapeDataString($relative).Replace('/', '\\')
}

function Ensure-TrailingSeparator {
    param([string]$Path)

    if ($Path.EndsWith([System.IO.Path]::DirectorySeparatorChar) -or $Path.EndsWith([System.IO.Path]::AltDirectorySeparatorChar)) {
        return $Path
    }

    return $Path + [System.IO.Path]::DirectorySeparatorChar
}

function Get-ModuleConfigFields {
    param([string]$ModuleName)

    $settings = $script:ModuleSettings[$ModuleName]
    $configPath = Join-Path $WorkspaceRoot (Join-Path $settings.SourceRoot $settings.ConfigManagerFile)
    $lines = Read-TextLines -Path $configPath
    $stopLine = ($lines | Select-String -Pattern 'public static void LoadAllConfigs' -List | Select-Object -First 1).LineNumber
    if (-not $stopLine) {
        $stopLine = $lines.Count
    }

    $result = @()
    for ($index = 0; $index -lt ($stopLine - 1); $index++) {
        $line = $lines[$index]
        if ($line -match '^\s*public static (?<type>[A-Za-z0-9_<>\[\]?]+) (?<name>[A-Za-z0-9_]+)\b(?:\s*=\s*(?<default>[^;]+))?;\s*(?://\s*(?<comment>.*))?$') {
            $name = $Matches['name']
            if ($script:IgnoredConfigFields -contains $name) {
                continue
            }

            $defaultValue = if ($Matches['default']) { $Matches['default'].Trim() } else { $null }
            $comment = if ($Matches['comment']) { $Matches['comment'].Trim() } else { $null }

            $result += [pscustomobject]@{
                Module = $ModuleName
                ConfigKey = $name
                ConfigType = $Matches['type']
                DefaultValue = $defaultValue
                Comment = $comment
                SourceFile = $configPath
                SourceLine = $index + 1
            }
        }
    }

    return $result
}

function Get-DictionaryBody {
    param(
        [string]$RawText,
        [string]$DictionaryName
    )

    $pattern = '(?s)' + [regex]::Escape($DictionaryName) + '\s*=\s*.*?\{(?<body>.*?)\n\s*\};'
    $match = [regex]::Match($RawText, $pattern)
    if (-not $match.Success) {
        throw "Failed to find dictionary '$DictionaryName'."
    }

    return $match.Groups['body'].Value
}

function Get-ConfigKeysFromCondition {
    param(
        [string]$ConditionText,
        [string]$ModuleName
    )

    $settings = $script:ModuleSettings[$ModuleName]
    $keys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

    foreach ($match in [regex]::Matches($ConditionText, 'IsFeatureEnabled\("(?<key>[^"]+)"\)')) {
        [void]$keys.Add($match.Groups['key'].Value)
    }

    foreach ($match in [regex]::Matches($ConditionText, [regex]::Escape($settings.ConfigManagerName) + '\.(?<member>[A-Za-z_][A-Za-z0-9_]*)')) {
        $member = $match.Groups['member'].Value
        if ($script:IgnoredManagerMembers -contains $member) {
            continue
        }

        [void]$keys.Add($member)
    }

    return @($keys)
}

function Get-PatchClassPath {
    param([string]$PatchExpression)

    if ($PatchExpression -match 'typeof\((?<class>Features\.[A-Za-z0-9_.]+)\)') {
        return $Matches['class']
    }

    if ($PatchExpression -match '(?<class>Features\.[A-Za-z0-9_.]+?Patch)\.') {
        return $Matches['class']
    }

    return $null
}

function Get-ModuleSourceFiles {
    param([string]$ModuleName)

    if ($script:ModuleSourceFilesCache.ContainsKey($ModuleName)) {
        return $script:ModuleSourceFilesCache[$ModuleName]
    }

    $settings = $script:ModuleSettings[$ModuleName]
    $sourceRoot = Join-Path $WorkspaceRoot $settings.SourceRoot
    $files = @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -Filter '*.cs' -File | Select-Object -ExpandProperty FullName)
    $script:ModuleSourceFilesCache[$ModuleName] = $files
    return $files
}

function Find-PatchFileByClassDefinition {
    param(
        [string]$ModuleName,
        [string]$ClassPath
    )

    if ([string]::IsNullOrWhiteSpace($ClassPath)) {
        return $null
    }

    $segments = @($ClassPath -split '\.')
    if ($segments.Count -lt 2) {
        return $null
    }

    $className = $segments[$segments.Count - 1]
    $namespaceSuffix = (($segments | Select-Object -SkipLast 1) -join '.')
    $classPattern = '\b(?:public|internal)\s+(?:static\s+)?(?:partial\s+)?class\s+' + [regex]::Escape($className) + '\b'
    $namespacePattern = 'namespace\s+[A-Za-z0-9_.]*' + [regex]::Escape($namespaceSuffix) + '\s*(?:\{|;)'

    foreach ($file in (Get-ModuleSourceFiles -ModuleName $ModuleName)) {
        $rawText = Read-TextFile -Path $file
        if ($rawText -notmatch $classPattern) {
            continue
        }

        if ($rawText -match $namespacePattern) {
            return $file
        }
    }

    foreach ($file in (Get-ModuleSourceFiles -ModuleName $ModuleName)) {
        $rawText = Read-TextFile -Path $file
        if ($rawText -match $classPattern) {
            return $file
        }
    }

    return $null
}

function Get-PatchClassName {
    param([string]$ClassPath)

    if ([string]::IsNullOrWhiteSpace($ClassPath)) {
        return $null
    }

    $segments = @($ClassPath -split '\.')
    if ($segments.Count -eq 0) {
        return $null
    }

    return $segments[$segments.Count - 1]
}

function Get-PatchClassScope {
    param(
        [string]$PatchFile,
        [string]$PatchClassPath
    )

    if ([string]::IsNullOrWhiteSpace($PatchFile) -or [string]::IsNullOrWhiteSpace($PatchClassPath)) {
        return $null
    }

    $cacheKey = "$PatchFile|$PatchClassPath"
    if ($script:PatchClassScopeCache.ContainsKey($cacheKey)) {
        return $script:PatchClassScopeCache[$cacheKey]
    }

    if (-not (Test-Path -LiteralPath $PatchFile)) {
        return $null
    }

    $className = Get-PatchClassName -ClassPath $PatchClassPath
    if ([string]::IsNullOrWhiteSpace($className)) {
        return $null
    }

    $rawText = Read-TextFile -Path $PatchFile
    $lines = Read-TextLines -Path $PatchFile
    $classPattern = '\b(?:public|internal)\s+(?:static\s+)?(?:partial\s+)?class\s+' + [regex]::Escape($className) + '\b'
    $classMatch = [regex]::Matches($rawText, $classPattern) | Select-Object -First 1
    if (-not $classMatch) {
        return $null
    }

    $classLine = Get-LineNumberFromIndex -Text $rawText -Index $classMatch.Index
    if ($null -eq $classLine) {
        return $null
    }

    $startLine = $classLine
    for ($lineIndex = $classLine - 2; $lineIndex -ge 0; $lineIndex--) {
        $trimmedLine = $lines[$lineIndex].Trim()
        if ([string]::IsNullOrWhiteSpace($trimmedLine)) {
            break
        }

        if ($trimmedLine.StartsWith('///') -or $trimmedLine.StartsWith('[')) {
            $startLine = $lineIndex + 1
            continue
        }

        break
    }

    $braceStart = $rawText.IndexOf('{', $classMatch.Index)
    if ($braceStart -lt 0) {
        return $null
    }

    $braceEnd = Get-MatchingIndex -Text $rawText -StartIndex $braceStart -OpenChar '{' -CloseChar '}'
    if ($braceEnd -lt 0) {
        return $null
    }

    $endLine = Get-LineNumberFromIndex -Text $rawText -Index $braceEnd
    $scopedText = ($lines[($startLine - 1)..($endLine - 1)] -join [Environment]::NewLine)

    $result = [pscustomobject]@{
        ClassName = $className
        StartLine = $startLine
        EndLine = $endLine
        RawText = $scopedText
    }

    $script:PatchClassScopeCache[$cacheKey] = $result
    return $result
}

function Resolve-PatchFilePath {
    param(
        [string]$ModuleName,
        [string]$PatchExpression
    )

    $classPath = Get-PatchClassPath -PatchExpression $PatchExpression

    if (-not $classPath) {
        return $null
    }

    $cacheKey = "$ModuleName|$classPath"
    if ($script:PatchFileResolutionCache.ContainsKey($cacheKey)) {
        return $script:PatchFileResolutionCache[$cacheKey]
    }

    $settings = $script:ModuleSettings[$ModuleName]
    $guessedPath = Join-Path $WorkspaceRoot (Join-Path $settings.SourceRoot (($classPath -replace '\.', '\\') + '.cs'))
    if (Test-Path -LiteralPath $guessedPath) {
        $script:PatchFileResolutionCache[$cacheKey] = $guessedPath
        return $guessedPath
    }

    $resolvedPath = Find-PatchFileByClassDefinition -ModuleName $ModuleName -ClassPath $classPath
    if (-not $resolvedPath) {
        $resolvedPath = $guessedPath
    }

    $script:PatchFileResolutionCache[$cacheKey] = $resolvedPath
    return $resolvedPath
}

function Get-RegistrationEntries {
    param(
        [string]$ModuleName,
        [string]$DictionaryName,
        [string]$RegistrationType
    )

    $settings = $script:ModuleSettings[$ModuleName]
    $mainPath = Join-Path $WorkspaceRoot (Join-Path $settings.SourceRoot $settings.MainFile)
    $rawText = Read-TextFile -Path $mainPath
    $body = Get-DictionaryBody -RawText $rawText -DictionaryName $DictionaryName
    $pattern = '(?s)\{\s*"(?<registrationKey>[^"]+)"\s*,\s*\((?<patchExpr>.*?),\s*\(\)\s*=>\s*(?<condition>.*?)\)\s*\}\s*,?'

    $entries = @()
    foreach ($match in [regex]::Matches($body, $pattern)) {
        $condition = ($match.Groups['condition'].Value -replace '\s+', ' ').Trim()
        $patchExpr = ($match.Groups['patchExpr'].Value -replace '\s+', ' ').Trim()
        $patchClassPath = Get-PatchClassPath -PatchExpression $patchExpr
        $patchClass = Get-PatchClassName -ClassPath $patchClassPath
        $entries += [pscustomobject]@{
            Module = $ModuleName
            RegistrationType = $RegistrationType
            RegistrationKey = $match.Groups['registrationKey'].Value
            PatchExpression = $patchExpr
            PatchClassPath = $patchClassPath
            PatchClass = $patchClass
            PatchIdentity = if ($patchClassPath) { $patchClassPath } else { $patchExpr }
            ConditionText = $condition
            ConditionConfigKeys = @(Get-ConfigKeysFromCondition -ConditionText $condition -ModuleName $ModuleName)
            PatchFile = Resolve-PatchFilePath -ModuleName $ModuleName -PatchExpression $patchExpr
            SourceFile = $mainPath
        }
    }

    return $entries
}

function Count-TypeParameters {
    param([string]$ParameterText)

    if ([string]::IsNullOrWhiteSpace($ParameterText)) {
        return 0
    }

    if ($ParameterText -match 'Type\.EmptyTypes') {
        return 0
    }

    return [regex]::Matches($ParameterText, 'typeof\(').Count
}

function Resolve-PresetTargetInfo {
    param([string]$TargetExpression)

    if ($script:PresetMethodMap.ContainsKey($TargetExpression)) {
        return $script:PresetMethodMap[$TargetExpression]
    }

    return @{ Display = $TargetExpression; Name = $TargetExpression; Kind = 'Unknown' }
}

function Get-OriginalMethodInfos {
    param([string]$RawText)

    $results = @()
    $pattern = '(?s)new OriginalMethodInfo\s*\{(?<body>.*?)\};'
    foreach ($match in [regex]::Matches($RawText, $pattern)) {
        $body = $match.Groups['body'].Value
        $typeMatch = [regex]::Match($body, 'Type\s*=\s*typeof\((?<type>[^)]+)\)')
        $nameMatch = [regex]::Match($body, 'MethodName\s*=\s*"(?<name>[^"]+)"')
        $paramsMatch = [regex]::Match($body, '(?s)Parameters\s*=\s*(?<params>Type\.EmptyTypes|new Type\[\]\s*\{.*?\})')
        if (-not ($typeMatch.Success -and $nameMatch.Success)) {
            continue
        }

        $parameterText = $null
        $parameterCount = 0
        if ($paramsMatch.Success) {
            $parameterText = ($paramsMatch.Groups['params'].Value -replace '\s+', ' ').Trim()
            $parameterCount = Count-TypeParameters -ParameterText $parameterText
        }

        $results += [pscustomobject]@{
            Source = 'PatchBuilder'
            TypeName = $typeMatch.Groups['type'].Value.Trim()
            MethodName = $nameMatch.Groups['name'].Value.Trim()
            ParameterText = $parameterText
            ParameterCount = $parameterCount
        }
    }

    return $results
}

function Get-HarmonyTargets {
    param([string]$RawText)

    $results = @()
    $pattern = '(?s)\[HarmonyPatch\(typeof\((?<type>[^)]+)\),\s*"(?<method>[^"]+)"(?:,\s*(?<params>Type\.EmptyTypes|new (?:System\.)?Type\[\]\s*\{.*?\}))?\)\]'
    foreach ($match in [regex]::Matches($RawText, $pattern)) {
        $parameterText = $null
        $parameterCount = $null
        if ($match.Groups['params'].Success) {
            $parameterText = ($match.Groups['params'].Value -replace '\s+', ' ').Trim()
            $parameterCount = Count-TypeParameters -ParameterText $parameterText
        }

        $results += [pscustomobject]@{
            Source = 'HarmonyPatch'
            TypeName = $match.Groups['type'].Value.Trim()
            MethodName = $match.Groups['method'].Value.Trim()
            ParameterText = $parameterText
            ParameterCount = $parameterCount
        }
    }

    return $results
}

function Get-ReplacementDefinitions {
    param([string]$RawText)

    $results = @()
    $pattern = '(?s)Add(?<kind>ExtensionMethodReplacement|InstanceMethodReplacement|LocalFunctionReplacement)\(\s*(?<target>[^,]+),\s*(?<replacement>[^,]+),\s*(?<occurrenceExpr>[A-Za-z0-9_]+)\s*\)'
    foreach ($match in [regex]::Matches($RawText, $pattern)) {
        $targetExpression = ($match.Groups['target'].Value -replace '\s+', ' ').Trim()
        $targetInfo = Resolve-PresetTargetInfo -TargetExpression $targetExpression
        $occurrenceExpression = ($match.Groups['occurrenceExpr'].Value -replace '\s+', ' ').Trim()
        $targetOccurrences = Resolve-OccurrenceExpression -OccurrenceExpression $occurrenceExpression -RawText $RawText -MatchIndex $match.Index
        $results += [pscustomobject]@{
            ReplacementKind = $match.Groups['kind'].Value
            TargetExpression = $targetExpression
            TargetDisplay = $targetInfo.Display
            TargetMethodName = $targetInfo.Name
            ReplacementExpression = ($match.Groups['replacement'].Value -replace '\s+', ' ').Trim()
            OccurrenceExpression = $occurrenceExpression
            TargetOccurrences = @($targetOccurrences)
        }
    }

    return $results
}

function Resolve-OccurrenceExpression {
    param(
        [string]$OccurrenceExpression,
        [string]$RawText,
        [int]$MatchIndex
    )

    if ($OccurrenceExpression -match '^\d+$') {
        return @([int]$OccurrenceExpression)
    }

    if ($OccurrenceExpression -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
        return @($OccurrenceExpression)
    }

    $scanStart = [Math]::Max(0, $MatchIndex - 400)
    $context = $RawText.Substring($scanStart, $MatchIndex - $scanStart)
    $forPattern = 'for\s*\(\s*int\s+' + [regex]::Escape($OccurrenceExpression) + '\s*=\s*(?<start>\d+)\s*;\s*' + [regex]::Escape($OccurrenceExpression) + '\s*(?<operator><=|<)\s*(?<end>\d+)\s*;\s*' + [regex]::Escape($OccurrenceExpression) + '\+\+'
    $forMatch = [regex]::Matches($context, $forPattern) | Select-Object -Last 1
    if ($forMatch) {
        $start = [int]$forMatch.Groups['start'].Value
        $end = [int]$forMatch.Groups['end'].Value
        if ($forMatch.Groups['operator'].Value -eq '<') {
            $end--
        }

        if ($end -ge $start) {
            return @($start..$end)
        }
    }

    return @($OccurrenceExpression)
}

function Get-ClassPatchBehaviorSummary {
    param([string]$RawText)

    $parts = New-Object System.Collections.Generic.List[string]
    if ($RawText -match '\[HarmonyPrefix\]') {
        $parts.Add('Prefix') | Out-Null
    }
    if ($RawText -match '\[HarmonyPostfix\]') {
        $parts.Add('Postfix') | Out-Null
    }
    if ($RawText -match 'ref\s+[A-Za-z0-9_<>\[\]?]+\s+__result' -or $RawText -match '__result\s*=') {
        $parts.Add('modifies return value') | Out-Null
    }
    if ($RawText -match 'return false;') {
        $parts.Add('can skip original method') | Out-Null
    }
    if ($RawText -match 'ref\s+[A-Za-z0-9_<>\[\]?]+\s+[A-Za-z0-9_]+' -and $RawText -notmatch '__result') {
        $parts.Add('may modify parameters') | Out-Null
    }

    if ($parts.Count -eq 0) {
        return 'Class patch behavior not auto-classified.'
    }

    return ($parts -join '; ')
}

function Get-PatchMetadata {
    param(
        [string]$ModuleName,
        [string]$PatchFile,
        [string]$PatchClassPath
    )

    $cacheKey = "$ModuleName|$PatchFile|$PatchClassPath"
    if ($script:PatchMetadataCache.ContainsKey($cacheKey)) {
        return $script:PatchMetadataCache[$cacheKey]
    }

    if (-not (Test-Path -LiteralPath $PatchFile)) {
        $result = [pscustomobject]@{
            Exists = $false
            PatchFile = $PatchFile
            PatchClassPath = $PatchClassPath
            ClassSummary = $null
            ContextStrategy = $null
            FeatureKeys = @()
            PatchBuilderTargets = @()
            HarmonyTargets = @()
            Replacements = @()
            ClassPatchBehavior = 'Patch file missing.'
            HasPrefix = $false
            HasPostfix = $false
            Warnings = @("Patch file not found: $PatchFile")
        }

        $script:PatchMetadataCache[$cacheKey] = $result
        return $result
    }

    $classScope = Get-PatchClassScope -PatchFile $PatchFile -PatchClassPath $PatchClassPath
    if ($null -eq $classScope) {
        $result = [pscustomobject]@{
            Exists = $false
            PatchFile = $PatchFile
            PatchClassPath = $PatchClassPath
            ClassSummary = $null
            ContextStrategy = $null
            FeatureKeys = @()
            PatchBuilderTargets = @()
            HarmonyTargets = @()
            Replacements = @()
            ClassPatchBehavior = 'Patch class not found in file.'
            HasPrefix = $false
            HasPostfix = $false
            Warnings = @("Patch class not found: $PatchClassPath in $PatchFile")
        }

        $script:PatchMetadataCache[$cacheKey] = $result
        return $result
    }

    $rawText = $classScope.RawText
    $classSummaryMatch = [regex]::Match($rawText, '(?s)/// <summary>\s*(?<text>.*?)\s*/// </summary>')
    $classSummary = if ($classSummaryMatch.Success) { ($classSummaryMatch.Groups['text'].Value -replace '\s*///\s*', ' ' -replace '\s+', ' ').Trim() } else { $null }
    $featureKeys = @(Get-ConfigKeysFromCondition -ConditionText $rawText -ModuleName $ModuleName)

    $contextStrategy = if ($rawText -match 'ActionPatchBase\.SetCharacterContext') {
        'ActionPatchBase'
    }
    elseif ($rawText -match 'CombatPatchBase\.SetCharacterContext') {
        'CombatPatchBase'
    }
    else {
        $null
    }

    $patchBuilderTargets = Get-OriginalMethodInfos -RawText $rawText
    $harmonyTargets = Get-HarmonyTargets -RawText $rawText
    $replacements = Get-ReplacementDefinitions -RawText $rawText

    $result = [pscustomobject]@{
        Exists = $true
        PatchFile = $PatchFile
        PatchClassPath = $PatchClassPath
        ClassSummary = $classSummary
        ContextStrategy = $contextStrategy
        FeatureKeys = @($featureKeys)
        PatchBuilderTargets = $patchBuilderTargets
        HarmonyTargets = $harmonyTargets
        Replacements = $replacements
        ClassPatchBehavior = Get-ClassPatchBehaviorSummary -RawText $rawText
        HasPrefix = $rawText -match '\[HarmonyPrefix\]'
        HasPostfix = $rawText -match '\[HarmonyPostfix\]'
        Warnings = @()
    }

    $script:PatchMetadataCache[$cacheKey] = $result
    return $result
}

function Convert-ToRegistrationWithBinding {
    param(
        $Registration,
        [string]$ConfigBinding
    )

    return [pscustomobject]@{
        Module = $Registration.Module
        RegistrationType = $Registration.RegistrationType
        RegistrationKey = $Registration.RegistrationKey
        PatchExpression = $Registration.PatchExpression
        PatchClassPath = $Registration.PatchClassPath
        PatchClass = $Registration.PatchClass
        PatchIdentity = $Registration.PatchIdentity
        ConditionText = $Registration.ConditionText
        ConditionConfigKeys = @($Registration.ConditionConfigKeys)
        PatchFile = $Registration.PatchFile
        SourceFile = $Registration.SourceFile
        ConfigBinding = $ConfigBinding
    }
}

function Get-InternalConsumptionRegistrations {
    param(
        [string]$ModuleName,
        [string]$ConfigKey,
        [object[]]$ModuleRegistrations
    )

    $results = @()
    foreach ($patchGroup in ($ModuleRegistrations | Group-Object -Property PatchIdentity)) {
        $registrationGroup = @($patchGroup.Group)
        if (@($registrationGroup | Where-Object { $_.ConditionConfigKeys -contains $ConfigKey }).Count -gt 0) {
            continue
        }

        $patchFile = $registrationGroup[0].PatchFile
        $patchClassPath = $registrationGroup[0].PatchClassPath
        $patchMetadata = Get-PatchMetadata -ModuleName $ModuleName -PatchFile $patchFile -PatchClassPath $patchClassPath
        if (@($patchMetadata.FeatureKeys) -notcontains $ConfigKey) {
            continue
        }

        $results += @($registrationGroup | ForEach-Object { Convert-ToRegistrationWithBinding -Registration $_ -ConfigBinding 'InternalConsumption' })
    }

    return @($results)
}

function Merge-TargetMethods {
    param(
        [object[]]$PatchBuilderTargets,
        [object[]]$HarmonyTargets
    )

    $dedupe = @{}
    foreach ($target in @($PatchBuilderTargets) + @($HarmonyTargets)) {
        if (-not $target) {
            continue
        }

        $propertyBag = $target.PSObject.Properties
        if ($propertyBag['TypeName'] -eq $null -or $propertyBag['MethodName'] -eq $null) {
            continue
        }

        $parameterCount = if ($propertyBag['ParameterCount']) { $target.ParameterCount } else { $null }
        $parameterText = if ($propertyBag['ParameterText']) { $target.ParameterText } else { $null }
        $source = if ($propertyBag['Source']) { $target.Source } else { 'Unknown' }
        $baseKey = '{0}::{1}' -f $target.TypeName, $target.MethodName
        $arity = if ($null -eq $parameterCount) { '?' } else { [string]$parameterCount }
        $key = '{0}::{1}' -f $baseKey, $arity
        $existingKeys = @($dedupe.Keys | Where-Object { $_ -like "$baseKey::*" })
        if ($existingKeys.Count -gt 0) {
            $preferredKey = $existingKeys | Where-Object { $_ -notlike '*::?' } | Select-Object -First 1
            if ($null -eq $parameterCount -and $preferredKey) {
                if ($dedupe[$preferredKey].DeclaredBy -notcontains $source) {
                    $dedupe[$preferredKey].DeclaredBy += $source
                }
                continue
            }

            $wildcardKey = $existingKeys | Where-Object { $_ -like '*::?' } | Select-Object -First 1
            if ($null -ne $parameterCount -and $wildcardKey) {
                $dedupe[$key] = [pscustomobject]@{
                    TypeName = $target.TypeName
                    MethodName = $target.MethodName
                    ParameterCount = $parameterCount
                    ParameterText = $parameterText
                    DeclaredBy = @($dedupe[$wildcardKey].DeclaredBy + $source | Select-Object -Unique)
                }
                $dedupe.Remove($wildcardKey)
                continue
            }
        }

        if (-not $dedupe.ContainsKey($key)) {
            $dedupe[$key] = [pscustomobject]@{
                TypeName = $target.TypeName
                MethodName = $target.MethodName
                ParameterCount = $parameterCount
                ParameterText = $parameterText
                DeclaredBy = @($source)
            }
        }
        elseif ($dedupe[$key].DeclaredBy -notcontains $source) {
            $dedupe[$key].DeclaredBy += $source
        }
    }

    return @($dedupe.Values)
}

function Resolve-GameSourceFile {
    param(
        [string]$GameSourceVersionRoot,
        [string]$TypeName
    )

    if ([string]::IsNullOrWhiteSpace($TypeName)) {
        return $null
    }

    $typeLeaf = $TypeName
    $lastDot = $TypeName.LastIndexOf('.')
    if ($lastDot -ge 0) {
        $namespacePath = $TypeName.Substring(0, $lastDot)
        $typeLeaf = $TypeName.Substring($lastDot + 1)
        $candidate = Join-Path $GameSourceVersionRoot (Join-Path $namespacePath ($typeLeaf + '.cs'))
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    $searchResult = Get-ChildItem -LiteralPath $GameSourceVersionRoot -Recurse -Filter ($typeLeaf + '.cs') -File | Select-Object -First 1
    if ($searchResult) {
        return $searchResult.FullName
    }

    return $null
}

function Get-TopLevelElementCount {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return 0
    }

    $depthParen = 0
    $depthBracket = 0
    $depthBrace = 0
    $depthAngle = 0
    $count = 1
    foreach ($character in $Text.ToCharArray()) {
        switch ($character) {
            '(' { $depthParen++ }
            ')' { if ($depthParen -gt 0) { $depthParen-- } }
            '[' { $depthBracket++ }
            ']' { if ($depthBracket -gt 0) { $depthBracket-- } }
            '{' { $depthBrace++ }
            '}' { if ($depthBrace -gt 0) { $depthBrace-- } }
            '<' { $depthAngle++ }
            '>' { if ($depthAngle -gt 0) { $depthAngle-- } }
            ',' {
                if ($depthParen -eq 0 -and $depthBracket -eq 0 -and $depthBrace -eq 0 -and $depthAngle -eq 0) {
                    $count++
                }
            }
        }
    }

    return $count
}

function Get-MatchingIndex {
    param(
        [string]$Text,
        [int]$StartIndex,
        [char]$OpenChar,
        [char]$CloseChar
    )

    $depth = 0
    for ($index = $StartIndex; $index -lt $Text.Length; $index++) {
        $character = $Text[$index]
        if ($character -eq $OpenChar) {
            $depth++
            continue
        }

        if ($character -eq $CloseChar) {
            $depth--
            if ($depth -eq 0) {
                return $index
            }
        }
    }

    return -1
}

function Get-LineNumberFromIndex {
    param(
        [string]$Text,
        [int]$Index
    )

    if ($Index -lt 0) {
        return $null
    }

    return ([regex]::Matches($Text.Substring(0, $Index), "`n").Count + 1)
}

function Get-MethodSnapshot {
    param(
        [string]$SourceFile,
        [string]$MethodName,
        [Nullable[int]]$ExpectedParameterCount
    )

    if (-not (Test-Path -LiteralPath $SourceFile)) {
        return $null
    }

    $rawText = Read-TextFile -Path $SourceFile
    $matches = [regex]::Matches($rawText, '\b' + [regex]::Escape($MethodName) + '\s*\(')
    $candidates = @()
    foreach ($match in $matches) {
        $lineStart = $rawText.LastIndexOf("`n", $match.Index)
        if ($lineStart -lt 0) {
            $lineStart = 0
        }
        else {
            $lineStart += 1
        }

        $parenStart = $rawText.IndexOf('(', $match.Index)
        if ($parenStart -lt 0) {
            continue
        }

        $parenEnd = Get-MatchingIndex -Text $rawText -StartIndex $parenStart -OpenChar '(' -CloseChar ')'
        if ($parenEnd -lt 0) {
            continue
        }

        $signatureSlice = $rawText.Substring($lineStart, [Math]::Min(400, $parenEnd - $lineStart + 1))
        if ($signatureSlice -notmatch '(public|private|protected|internal|static|override|virtual|sealed|async|unsafe|extern)') {
            continue
        }

        $parameterText = $rawText.Substring($parenStart + 1, $parenEnd - $parenStart - 1)
        $parameterCount = Get-TopLevelElementCount -Text $parameterText
        if ($null -ne $ExpectedParameterCount -and $parameterCount -ne [int]$ExpectedParameterCount) {
            continue
        }

        $braceStart = $rawText.IndexOf('{', $parenEnd)
        if ($braceStart -lt 0 -or $braceStart -gt ($parenEnd + 500)) {
            continue
        }

        $braceEnd = Get-MatchingIndex -Text $rawText -StartIndex $braceStart -OpenChar '{' -CloseChar '}'
        if ($braceEnd -lt 0) {
            continue
        }

        $candidateText = $rawText.Substring($lineStart, $braceEnd - $lineStart + 1).TrimEnd()
        $candidates += [pscustomobject]@{
            MethodText = $candidateText
            ParameterCount = $parameterCount
            StartLine = Get-LineNumberFromIndex -Text $rawText -Index $lineStart
            EndLine = Get-LineNumberFromIndex -Text $rawText -Index $braceEnd
        }
    }

    if ($candidates.Count -eq 0) {
        return $null
    }

    return $candidates[0]
}

function Get-ReplacementOccurrenceSummary {
    param(
        [object[]]$Replacements,
        [string]$MethodText
    )

    if (-not $MethodText) {
        return @()
    }

    $groups = $Replacements | Group-Object -Property TargetMethodName
    $results = @()
    foreach ($group in $groups) {
        $methodName = $group.Name
        if ([string]::IsNullOrWhiteSpace($methodName)) {
            continue
        }

        $count = [regex]::Matches($MethodText, '\b' + [regex]::Escape($methodName) + '\s*\(').Count
        $expectedOccurrences = @($group.Group | ForEach-Object { @($_.TargetOccurrences) } | Sort-Object -Unique)
        $results += [pscustomobject]@{
            TargetMethodName = $methodName
            ExpectedOccurrences = $expectedOccurrences
            ObservedNameMatches = $count
        }
    }

    return $results
}

function Get-PatchKindDescription {
    param(
        $RegistrationTypes,
        $PatchMetadata
    )

    $normalizedRegistrationTypes = @($RegistrationTypes | ForEach-Object { [string]$_ })

    $hasClass = $normalizedRegistrationTypes -contains 'ClassPatch'
    $hasBuilder = $normalizedRegistrationTypes -contains 'PatchBuilder'
    if ($hasClass -and $hasBuilder) {
        return 'Hybrid'
    }
    if ($hasClass) {
        return 'ClassPatch'
    }
    if ($hasBuilder) {
        return 'PatchBuilder'
    }

    if (@($PatchMetadata.HarmonyTargets).Count -gt 0 -and @($PatchMetadata.PatchBuilderTargets).Count -gt 0) {
        return 'Hybrid'
    }
    if (@($PatchMetadata.HarmonyTargets).Count -gt 0) {
        return 'ClassPatch'
    }
    if (@($PatchMetadata.PatchBuilderTargets).Count -gt 0) {
        return 'PatchBuilder'
    }

    return 'Unknown'
}

function Get-PatchWorkSummary {
    param(
        $RegistrationTypes,
        $PatchMetadata
    )

    $normalizedRegistrationTypes = @($RegistrationTypes | ForEach-Object { [string]$_ })
    $kind = Get-PatchKindDescription -RegistrationTypes $normalizedRegistrationTypes -PatchMetadata $PatchMetadata
    switch ($kind) {
        'Hybrid' {
            $contextStrategy = if ([string]::IsNullOrWhiteSpace($PatchMetadata.ContextStrategy)) {
                'none'
            }
            else {
                $PatchMetadata.ContextStrategy
            }

            $replacementItems = @($PatchMetadata.Replacements | ForEach-Object {
                    $occurrenceDisplay = if (@($_.TargetOccurrences).Count -gt 0) { @($_.TargetOccurrences) -join ',' } else { $_.OccurrenceExpression }
                    "$($_.TargetDisplay) occurrence $occurrenceDisplay"
                })
            $replacementSummary = if (@($replacementItems).Count -gt 0) {
                $replacementItems -join '; '
            }
            else {
                'No replacement calls auto-detected.'
            }

            return "Context injection via $contextStrategy; transpiler replacements: $replacementSummary"
        }
        'PatchBuilder' {
            if (@($PatchMetadata.Replacements).Count -eq 0) {
                return 'PatchBuilder target detected, but replacement calls were not auto-detected.'
            }

            $replacementItems = @($PatchMetadata.Replacements | ForEach-Object {
                    $occurrenceDisplay = if (@($_.TargetOccurrences).Count -gt 0) { @($_.TargetOccurrences) -join ',' } else { $_.OccurrenceExpression }
                    "$($_.TargetDisplay) occurrence $occurrenceDisplay"
                })
            return "Transpiler replaces $($replacementItems -join '; ')"
        }
        'ClassPatch' {
            return $PatchMetadata.ClassPatchBehavior
        }
        default {
            return 'Patch behavior could not be auto-classified.'
        }
    }
}

function Copy-GameSourceSnapshot {
    param(
        [string]$SourceRoot,
        [string]$DestinationRoot
    )

    Ensure-Directory -Path $DestinationRoot | Out-Null
    Write-Status "Copying game source snapshot to $DestinationRoot"
    $arguments = @(
        $SourceRoot,
        $DestinationRoot,
        '/E',
        '/R:1',
        '/W:1',
        '/NFL',
        '/NDL',
        '/NJH',
        '/NJS',
        '/NP'
    )

    & robocopy @arguments | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed with exit code $LASTEXITCODE"
    }
}

function Export-MarkdownInventory {
    param(
        [object[]]$RegistryEntries,
        [string]$Version,
        [string]$MarkdownPath
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Patch Baseline Inventory - $Version") | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add("Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')") | Out-Null
    $lines.Add('') | Out-Null

    foreach ($moduleGroup in ($RegistryEntries | Group-Object -Property Module | Sort-Object Name)) {
        $lines.Add("## $($moduleGroup.Name)") | Out-Null
        $lines.Add('') | Out-Null

        foreach ($entry in ($moduleGroup.Group | Sort-Object ConfigKey)) {
            $lines.Add("### $($entry.ConfigKey)") | Out-Null
            $lines.Add('') | Out-Null
            $lines.Add("- Config type: $($entry.ConfigType)") | Out-Null
            if ($entry.Comment) {
                $lines.Add("- Config comment: $($entry.Comment)") | Out-Null
            }
            if (@($entry.PatchEntries).Count -eq 0) {
                $lines.Add('- Patch status: no direct patch mapping detected') | Out-Null
            }
            else {
                foreach ($patch in $entry.PatchEntries) {
                    $bindingSuffix = if ($patch.PSObject.Properties['ConfigBinding'] -and $patch.ConfigBinding -ne 'DirectRegistration') { " [$($patch.ConfigBinding)]" } else { '' }
                    $lines.Add("- Patch: $($patch.PatchKind) via $($patch.RegistrationType)$bindingSuffix :: $($patch.PatchClass)") | Out-Null
                    $lines.Add("- How it patches: $($patch.WorkSummary)") | Out-Null
                    if (@($patch.TargetMethods).Count -gt 0) {
                        $lines.Add("- Target methods: $((@($patch.TargetMethods | ForEach-Object { $_.Signature }) -join '; '))") | Out-Null
                    }
                }
            }
            if (@($entry.Warnings).Count -gt 0) {
                $lines.Add("- Warnings: $((@($entry.Warnings) -join ' | '))") | Out-Null
            }
            $lines.Add('') | Out-Null
        }
    }

    Set-Content -LiteralPath $MarkdownPath -Value $lines -Encoding UTF8
}

function Export-RegistryCsv {
    param(
        [object[]]$RegistryEntries,
        [string]$CsvPath
    )

    $rows = foreach ($entry in $RegistryEntries) {
        if (@($entry.PatchEntries).Count -eq 0) {
            [pscustomobject]@{
                Module = $entry.Module
                ConfigKey = $entry.ConfigKey
                ConfigType = $entry.ConfigType
                PatchKind = 'Unmapped'
                PatchClass = ''
                RegistrationType = ''
                ConfigBinding = ''
                TargetMethods = ''
                WorkSummary = ''
                WarningCount = @($entry.Warnings).Count
            }
            continue
        }

        foreach ($patch in $entry.PatchEntries) {
            [pscustomobject]@{
                Module = $entry.Module
                ConfigKey = $entry.ConfigKey
                ConfigType = $entry.ConfigType
                PatchKind = $patch.PatchKind
                PatchClass = $patch.PatchClass
                RegistrationType = $patch.RegistrationType
                ConfigBinding = if ($patch.PSObject.Properties['ConfigBinding']) { $patch.ConfigBinding } else { 'DirectRegistration' }
                TargetMethods = @($patch.TargetMethods | ForEach-Object { $_.Signature }) -join '; '
                WorkSummary = $patch.WorkSummary
                WarningCount = (@($entry.Warnings).Count + @($patch.Warnings).Count)
            }
        }
    }

    $rows | Export-Csv -LiteralPath $CsvPath -NoTypeInformation -Encoding UTF8
}

function Import-VersionRegistry {
    param([string]$VersionName)

    $versionRoot = Join-Path $OutputRoot (Join-Path 'versions' $VersionName)
    $registryPath = Join-Path $versionRoot 'registry.json'
    if (-not (Test-Path -LiteralPath $registryPath)) {
        throw "Registry file not found for version '$VersionName': $registryPath"
    }

    $rawRegistry = ConvertFrom-Json -InputObject (Read-TextFile -Path $registryPath)
    $registryItems = @($rawRegistry | ForEach-Object { $_ })

    return [pscustomobject]@{
        Version = $VersionName
        Root = $versionRoot
        Registry = $registryItems
    }
}

function ConvertTo-ComparableJson {
    param(
        $Value,
        [int]$Depth = 20
    )

    if ($null -eq $Value) {
        return ''
    }

    return ($Value | ConvertTo-Json -Depth $Depth -Compress)
}

function Normalize-TargetMethodsForComparison {
    param($TargetMethods)

    $normalized = @()
    foreach ($group in (@($TargetMethods) | Group-Object { '{0}::{1}' -f $_.TypeName, $_.MethodName })) {
        $items = @($group.Group)
        $specificItems = @($items | Where-Object { $null -ne $_.ParameterCount })
        if ($specificItems.Count -gt 0) {
            $items = $specificItems
        }

        $normalized += @($items | Group-Object Signature | ForEach-Object { $_.Group | Select-Object -First 1 })
    }

    return @($normalized)
}

function Get-TargetMethodLookup {
    param($TargetMethods)

    $lookup = @{}
    foreach ($group in (Normalize-TargetMethodsForComparison -TargetMethods $TargetMethods | Group-Object { '{0}::{1}' -f $_.TypeName, $_.MethodName })) {
        $items = @($group.Group)
        if ($items.Count -eq 1) {
            $lookup[$group.Name] = $items[0]
            continue
        }

        foreach ($item in $items) {
            $lookup[$item.Signature] = $item
        }
    }

    return $lookup
}

function Get-RecommendedAction {
    param([string[]]$Reasons)

    $reasonSet = @($Reasons | Select-Object -Unique)
    if ($reasonSet.Count -eq 0) {
        return 'no-update-needed'
    }
    if ($reasonSet -contains 'target-method-removed' -or $reasonSet -contains 'patch-removed' -or $reasonSet -contains 'target-method-added') {
        return 'patch-redesign-needed'
    }
    if ($reasonSet -contains 'method-code-changed' -or $reasonSet -contains 'occurrence-profile-changed' -or $reasonSet -contains 'replacement-definition-changed' -or $reasonSet -contains 'patch-kind-changed') {
        return 'patch-change-needed'
    }
    if ($reasonSet -contains 'condition-config-changed' -or $reasonSet -contains 'registration-type-changed' -or $reasonSet -contains 'work-summary-changed' -or $reasonSet -contains 'patch-added') {
        return 'doc-only'
    }

    return 'pending-review'
}

function Compare-TargetMethods {
    param(
        $BaseTargetMethods,
        $TargetTargetMethods
    )

    $baseLookup = Get-TargetMethodLookup -TargetMethods $BaseTargetMethods
    $targetLookup = Get-TargetMethodLookup -TargetMethods $TargetTargetMethods
    $allKeys = @($baseLookup.Keys + $targetLookup.Keys | Sort-Object -Unique)
    $comparisons = @()

    foreach ($key in $allKeys) {
        $baseMethod = if ($baseLookup.ContainsKey($key)) { $baseLookup[$key] } else { $null }
        $targetMethod = if ($targetLookup.ContainsKey($key)) { $targetLookup[$key] } else { $null }
        $reasons = @()

        if ($null -eq $baseMethod) {
            $reasons += 'target-method-added'
        }
        elseif ($null -eq $targetMethod) {
            $reasons += 'target-method-removed'
        }
        else {
            if ($baseMethod.Signature -ne $targetMethod.Signature) {
                $reasons += 'signature-changed'
            }
            if ($baseMethod.MethodCodeHash -ne $targetMethod.MethodCodeHash) {
                $reasons += 'method-code-changed'
            }
            if ((ConvertTo-ComparableJson -Value $baseMethod.ObservedOccurrences) -ne (ConvertTo-ComparableJson -Value $targetMethod.ObservedOccurrences)) {
                $reasons += 'occurrence-profile-changed'
            }
        }

        $comparisons += [pscustomobject]@{
            Key = $key
            Status = if ($reasons.Count -eq 0) { 'unchanged' } else { 'changed' }
            Reasons = @($reasons)
            Base = $baseMethod
            Target = $targetMethod
        }
    }

    return @($comparisons)
}

function Get-PatchEntryIdentity {
    param($PatchEntry)

    if ($null -eq $PatchEntry) {
        return $null
    }

    if ($PatchEntry.PSObject.Properties['PatchClassPath'] -and -not [string]::IsNullOrWhiteSpace($PatchEntry.PatchClassPath)) {
        return $PatchEntry.PatchClassPath
    }

    if ($PatchEntry.PSObject.Properties['PatchClass'] -and -not [string]::IsNullOrWhiteSpace($PatchEntry.PatchClass)) {
        return $PatchEntry.PatchClass
    }

    if ($PatchEntry.PSObject.Properties['PatchFile'] -and -not [string]::IsNullOrWhiteSpace($PatchEntry.PatchFile)) {
        return $PatchEntry.PatchFile
    }

    return ''
}

function Compare-PatchEntries {
    param(
        $BasePatchEntries,
        $TargetPatchEntries
    )

    $baseLookup = @{}
    foreach ($patch in @($BasePatchEntries)) {
        $baseLookup[('{0}|{1}' -f (Get-PatchEntryIdentity -PatchEntry $patch), $patch.RegistrationType)] = $patch
    }

    $targetLookup = @{}
    foreach ($patch in @($TargetPatchEntries)) {
        $targetLookup[('{0}|{1}' -f (Get-PatchEntryIdentity -PatchEntry $patch), $patch.RegistrationType)] = $patch
    }

    $allKeys = @($baseLookup.Keys + $targetLookup.Keys | Sort-Object -Unique)
    $comparisons = @()
    foreach ($key in $allKeys) {
        $basePatch = if ($baseLookup.ContainsKey($key)) { $baseLookup[$key] } else { $null }
        $targetPatch = if ($targetLookup.ContainsKey($key)) { $targetLookup[$key] } else { $null }
        $reasons = @()
        $targetMethodComparisons = @()

        if ($null -eq $basePatch) {
            $reasons += 'patch-added'
        }
        elseif ($null -eq $targetPatch) {
            $reasons += 'patch-removed'
        }
        else {
            if ($basePatch.PatchKind -ne $targetPatch.PatchKind) {
                $reasons += 'patch-kind-changed'
            }
            if ($basePatch.RegistrationType -ne $targetPatch.RegistrationType) {
                $reasons += 'registration-type-changed'
            }
            if ((ConvertTo-ComparableJson -Value $basePatch.ConditionConfigKeys) -ne (ConvertTo-ComparableJson -Value $targetPatch.ConditionConfigKeys)) {
                $reasons += 'condition-config-changed'
            }
            if ((ConvertTo-ComparableJson -Value $basePatch.Replacements) -ne (ConvertTo-ComparableJson -Value $targetPatch.Replacements)) {
                $reasons += 'replacement-definition-changed'
            }
            if ($basePatch.WorkSummary -ne $targetPatch.WorkSummary) {
                $reasons += 'work-summary-changed'
            }

            $targetMethodComparisons = Compare-TargetMethods -BaseTargetMethods $basePatch.TargetMethods -TargetTargetMethods $targetPatch.TargetMethods
            if (@($targetMethodComparisons | Where-Object { $_.Status -ne 'unchanged' }).Count -gt 0) {
                $reasons += @($targetMethodComparisons | Where-Object { $_.Status -ne 'unchanged' } | ForEach-Object { $_.Reasons })
            }
        }

        $comparisons += [pscustomobject]@{
            Key = $key
            Status = if ($reasons.Count -eq 0) { 'unchanged' } else { 'review-needed' }
            RecommendedAction = Get-RecommendedAction -Reasons $reasons
            Reasons = @($reasons | Select-Object -Unique)
            Base = $basePatch
            Target = $targetPatch
            TargetMethodComparisons = @($targetMethodComparisons)
        }
    }

    return @($comparisons)
}

function Compare-RegistryEntries {
    param(
        $BaseRegistry,
        $TargetRegistry
    )

    $baseLookup = @{}
    foreach ($entry in @($BaseRegistry)) {
        $baseLookup[('{0}|{1}' -f $entry.Module, $entry.ConfigKey)] = $entry
    }

    $targetLookup = @{}
    foreach ($entry in @($TargetRegistry)) {
        $targetLookup[('{0}|{1}' -f $entry.Module, $entry.ConfigKey)] = $entry
    }

    $allKeys = @($baseLookup.Keys + $targetLookup.Keys | Sort-Object -Unique)
    $comparisons = @()
    foreach ($key in $allKeys) {
        $baseEntry = if ($baseLookup.ContainsKey($key)) { $baseLookup[$key] } else { $null }
        $targetEntry = if ($targetLookup.ContainsKey($key)) { $targetLookup[$key] } else { $null }
        $reasons = @()
        $patchComparisons = @()

        if ($null -eq $baseEntry) {
            $reasons += 'config-added'
        }
        elseif ($null -eq $targetEntry) {
            $reasons += 'config-removed'
        }
        else {
            if ($baseEntry.ConfigType -ne $targetEntry.ConfigType) {
                $reasons += 'config-type-changed'
            }
            $patchComparisons = Compare-PatchEntries -BasePatchEntries $baseEntry.PatchEntries -TargetPatchEntries $targetEntry.PatchEntries
            if (@($patchComparisons | Where-Object { $_.Status -ne 'unchanged' }).Count -gt 0) {
                $reasons += @($patchComparisons | Where-Object { $_.Status -ne 'unchanged' } | ForEach-Object { $_.Reasons })
            }
        }

        $moduleName = if ($baseEntry) { $baseEntry.Module } else { $targetEntry.Module }
        $configKey = if ($baseEntry) { $baseEntry.ConfigKey } else { $targetEntry.ConfigKey }

        $comparisons += [pscustomobject]@{
            Module = $moduleName
            ConfigKey = $configKey
            Status = if ($reasons.Count -eq 0) { 'unchanged' } else { 'review-needed' }
            RecommendedAction = Get-RecommendedAction -Reasons $reasons
            Reasons = @($reasons | Select-Object -Unique)
            Base = $baseEntry
            Target = $targetEntry
            PatchComparisons = @($patchComparisons)
        }
    }

    return @($comparisons)
}

function Export-ComparisonMarkdown {
    param(
        [object[]]$EntryComparisons,
        [string]$BaseVersionName,
        [string]$TargetVersionName,
        [string]$MarkdownPath
    )

    $summary = [ordered]@{
        unchanged = @($EntryComparisons | Where-Object { $_.Status -eq 'unchanged' }).Count
        reviewNeeded = @($EntryComparisons | Where-Object { $_.Status -ne 'unchanged' }).Count
        patchChangeNeeded = @($EntryComparisons | Where-Object { $_.RecommendedAction -eq 'patch-change-needed' }).Count
        patchRedesignNeeded = @($EntryComparisons | Where-Object { $_.RecommendedAction -eq 'patch-redesign-needed' }).Count
        docOnly = @($EntryComparisons | Where-Object { $_.RecommendedAction -eq 'doc-only' }).Count
    }

    $lines = @(
        "# Patch Baseline Compare - $BaseVersionName -> $TargetVersionName",
        '',
        "Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
        '',
        '## Summary',
        '',
        "- Unchanged: $($summary.unchanged)",
        "- Review Needed: $($summary.reviewNeeded)",
        "- Patch Change Needed: $($summary.patchChangeNeeded)",
        "- Patch Redesign Needed: $($summary.patchRedesignNeeded)",
        "- Doc Only: $($summary.docOnly)",
        ''
    )

    foreach ($moduleGroup in ($EntryComparisons | Group-Object Module | Sort-Object Name)) {
        $lines += "## $($moduleGroup.Name)"
        $lines += ''
        foreach ($entry in ($moduleGroup.Group | Sort-Object ConfigKey)) {
            $lines += "### $($entry.ConfigKey)"
            $lines += ''
            $lines += "- Status: $($entry.Status)"
            $lines += "- Recommended Action: $($entry.RecommendedAction)"
            if (@($entry.Reasons).Count -gt 0) {
                $lines += "- Reasons: $((@($entry.Reasons) -join '; '))"
            }
            foreach ($patchComparison in @($entry.PatchComparisons | Where-Object { $_.Status -ne 'unchanged' })) {
                $patchClass = if ($patchComparison.Target) { $patchComparison.Target.PatchClass } elseif ($patchComparison.Base) { $patchComparison.Base.PatchClass } else { $patchComparison.Key }
                $lines += "- Patch Diff: $patchClass => $((@($patchComparison.Reasons) -join '; '))"
            }
            $lines += ''
        }
    }

    Set-Content -LiteralPath $MarkdownPath -Value $lines -Encoding UTF8
}

function Export-UpdateReviewTemplate {
    param(
        [object[]]$EntryComparisons,
        [string]$TemplatePath
    )

    $templateRows = foreach ($entry in $EntryComparisons) {
        [pscustomobject]@{
            Module = $entry.Module
            ConfigKey = $entry.ConfigKey
            ComparisonStatus = $entry.Status
            RecommendedAction = $entry.RecommendedAction
            Reasons = @($entry.Reasons)
            ReviewState = 'pending'
            FinalDecision = ''
            Notes = ''
        }
    }

    $templateRows | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $TemplatePath -Encoding UTF8
}

function Invoke-Collect {
    param(
        [string]$SelectedVersion,
        [string[]]$SelectedModules,
        [string[]]$IncludedConfigKeys
    )

    if ([string]::IsNullOrWhiteSpace($SelectedVersion)) {
        $SelectedVersion = Get-Date -Format 'yyyyMMdd-HHmmss'
    }

    $versionRoot = Ensure-Directory -Path (Join-Path $OutputRoot (Join-Path 'versions' $SelectedVersion))
    $functionSnapshotRoot = Ensure-Directory -Path (Join-Path $versionRoot 'function-snapshots')
    $gameSourceSnapshotRoot = Join-Path $versionRoot 'game-source'

    $configuredGameSourceRoot = Resolve-ConfiguredGameSourceRoot -WorkspaceRootPath $WorkspaceRoot -OverrideRoot $GameSourceRoot
    if (-not (Test-Path -LiteralPath $configuredGameSourceRoot)) {
        throw "Game source root not found: $configuredGameSourceRoot. Pass -GameSourceRoot or set <PatchBaselineGameSourceRoot> in Directory.Build.props."
    }

    $effectiveGameSourceRoot = $configuredGameSourceRoot
    if ($CopyGameSource) {
        Copy-GameSourceSnapshot -SourceRoot $configuredGameSourceRoot -DestinationRoot $gameSourceSnapshotRoot
        $effectiveGameSourceRoot = $gameSourceSnapshotRoot
    }

    $allRegistrations = @()
    $registryEntries = @()
    foreach ($moduleName in $SelectedModules) {
        Write-Status "Collecting config and registration data for $moduleName"
        $configFields = Get-ModuleConfigFields -ModuleName $moduleName
        $classRegistrations = Get-RegistrationEntries -ModuleName $moduleName -DictionaryName 'patchConfigMappings' -RegistrationType 'ClassPatch'
        $builderRegistrations = Get-RegistrationEntries -ModuleName $moduleName -DictionaryName 'patchBuilderMappings' -RegistrationType 'PatchBuilder'
        $moduleRegistrations = @($classRegistrations + $builderRegistrations)
        $allRegistrations += $moduleRegistrations

        foreach ($field in $configFields) {
            if ($IncludedConfigKeys -and ($IncludedConfigKeys -notcontains $field.ConfigKey)) {
                continue
            }

            $registrations = @($moduleRegistrations | Where-Object { $_.ConditionConfigKeys -contains $field.ConfigKey })
            if (@($registrations).Count -eq 0) {
                $registrations = @(Get-InternalConsumptionRegistrations -ModuleName $moduleName -ConfigKey $field.ConfigKey -ModuleRegistrations $moduleRegistrations)
            }
            $patchEntries = @()
            $entryWarnings = @()

            foreach ($patchGroup in ($registrations | Group-Object -Property PatchIdentity)) {
                $registrationGroup = @($patchGroup.Group)
                $patchFile = $registrationGroup[0].PatchFile
                $patchClassPath = $registrationGroup[0].PatchClassPath
                $patchMetadata = Get-PatchMetadata -ModuleName $moduleName -PatchFile $patchFile -PatchClassPath $patchClassPath
                $registrationTypes = @($registrationGroup | ForEach-Object { $_.RegistrationType } | Sort-Object -Unique)
                $patchClass = if ($registrationGroup[0].PatchClass) { $registrationGroup[0].PatchClass } else { 'UnknownPatch' }
                $configBinding = if ($registrationGroup[0].PSObject.Properties['ConfigBinding']) { $registrationGroup[0].ConfigBinding } else { 'DirectRegistration' }
                $targetMethods = @()
                $entryWarnings += @($patchMetadata.Warnings)

                foreach ($targetMethod in (Merge-TargetMethods -PatchBuilderTargets $patchMetadata.PatchBuilderTargets -HarmonyTargets $patchMetadata.HarmonyTargets)) {
                    $resolvedSourceFile = Resolve-GameSourceFile -GameSourceVersionRoot $effectiveGameSourceRoot -TypeName $targetMethod.TypeName
                    $snapshot = if ($resolvedSourceFile) { Get-MethodSnapshot -SourceFile $resolvedSourceFile -MethodName $targetMethod.MethodName -ExpectedParameterCount $targetMethod.ParameterCount } else { $null }
                    $signature = '{0}.{1}({2})' -f $targetMethod.TypeName, $targetMethod.MethodName, ($(if ($null -eq $targetMethod.ParameterCount) { '?' } else { $targetMethod.ParameterCount }))
                    $snapshotPath = $null
                    $methodCode = $null
                    $methodHash = $null
                    $occurrenceSummary = @()

                    if ($snapshot) {
                        $methodCode = $snapshot.MethodText
                        $methodHash = Get-TextHash -Text $methodCode
                        $moduleSnapshotRoot = Ensure-Directory -Path (Join-Path $functionSnapshotRoot $moduleName)
                        $typeSnapshotRoot = Ensure-Directory -Path (Join-Path $moduleSnapshotRoot (Convert-ToSafeName -Text $targetMethod.TypeName))
                        $snapshotPath = Join-Path $typeSnapshotRoot ((Convert-ToSafeName -Text $targetMethod.MethodName) + '.cs')
                        Set-Content -LiteralPath $snapshotPath -Value $methodCode -Encoding UTF8
                        $occurrenceSummary = Get-ReplacementOccurrenceSummary -Replacements $patchMetadata.Replacements -MethodText $methodCode
                    }
                    else {
                        $entryWarnings += "Failed to resolve method snapshot for $signature"
                    }

                    $targetMethods += [pscustomobject]@{
                        Signature = $signature
                        TypeName = $targetMethod.TypeName
                        MethodName = $targetMethod.MethodName
                        ParameterCount = $targetMethod.ParameterCount
                        ParameterText = $targetMethod.ParameterText
                        DeclaredBy = $targetMethod.DeclaredBy
                        SourceFile = $resolvedSourceFile
                        SnapshotFile = $snapshotPath
                        StartLine = if ($snapshot) { $snapshot.StartLine } else { $null }
                        EndLine = if ($snapshot) { $snapshot.EndLine } else { $null }
                        MethodCode = $methodCode
                        MethodCodeHash = $methodHash
                        ObservedOccurrences = @($occurrenceSummary)
                    }
                }

                $patchKind = Get-PatchKindDescription -RegistrationTypes $registrationTypes -PatchMetadata $patchMetadata
                $registrationKeys = @($registrationGroup | ForEach-Object { $_.RegistrationKey } | Sort-Object -Unique)
                $conditionTexts = @($registrationGroup | ForEach-Object { $_.ConditionText } | Sort-Object -Unique)
                $conditionConfigKeys = @($registrationGroup | ForEach-Object { @($_.ConditionConfigKeys) } | Sort-Object -Unique)
                $workSummary = Get-PatchWorkSummary -RegistrationTypes $registrationTypes -PatchMetadata $patchMetadata

                $patchEntries += [pscustomobject]@{
                    PatchFile = $patchFile
                    PatchClass = $patchClass
                    PatchClassPath = $patchClassPath
                    ConfigBinding = $configBinding
                    PatchKind = $patchKind
                    RegistrationType = ($registrationTypes -join '+')
                    RegistrationKeys = $registrationKeys
                    ConditionText = $conditionTexts
                    ConditionConfigKeys = $conditionConfigKeys
                    ContextStrategy = $patchMetadata.ContextStrategy
                    WorkSummary = $workSummary
                    TargetMethods = @($targetMethods)
                    Replacements = @($patchMetadata.Replacements)
                    ClassSummary = $patchMetadata.ClassSummary
                    Warnings = @($patchMetadata.Warnings)
                }
            }

            $registryEntries += [pscustomobject]@{
                Module = $field.Module
                ConfigKey = $field.ConfigKey
                ConfigType = $field.ConfigType
                DefaultValue = $field.DefaultValue
                Comment = $field.Comment
                SourceFile = $field.SourceFile
                SourceLine = $field.SourceLine
                PatchEntries = @($patchEntries)
                Warnings = @($entryWarnings)
            }
        }
    }

    $registryPath = Join-Path $versionRoot 'registry.json'
    $csvPath = Join-Path $versionRoot 'registry.csv'
    $markdownPath = Join-Path $versionRoot 'inventory.md'
    $manifestPath = Join-Path $versionRoot 'manifest.json'

    $manifest = [pscustomobject]@{
        Version = $SelectedVersion
        GeneratedAt = (Get-Date).ToString('s')
        WorkspaceRoot = $WorkspaceRoot
        GameSourceRoot = $effectiveGameSourceRoot
        CopiedGameSource = [bool]$CopyGameSource
        Modules = $SelectedModules
        IncludedConfigKeys = $IncludedConfigKeys
        EntryCount = $registryEntries.Count
    }

    $registryEntries | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $registryPath -Encoding UTF8
    $manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    Export-RegistryCsv -RegistryEntries $registryEntries -CsvPath $csvPath
    Export-MarkdownInventory -RegistryEntries $registryEntries -Version $SelectedVersion -MarkdownPath $markdownPath

    Write-Status "Registry written to $registryPath"
    Write-Status "Inventory written to $markdownPath"
    return [pscustomobject]@{
        Version = $SelectedVersion
        VersionRoot = $versionRoot
        RegistryPath = $registryPath
        InventoryPath = $markdownPath
        ManifestPath = $manifestPath
    }
}

function Invoke-Compare {
    param(
        [string]$CompareBaseVersion,
        [string]$CompareTargetVersion
    )

    if ([string]::IsNullOrWhiteSpace($CompareBaseVersion) -or [string]::IsNullOrWhiteSpace($CompareTargetVersion)) {
        throw 'Compare requires both -BaseVersion and -TargetVersion.'
    }

    $baseVersionData = Import-VersionRegistry -VersionName $CompareBaseVersion
    $targetVersionData = Import-VersionRegistry -VersionName $CompareTargetVersion
    $entryComparisons = Compare-RegistryEntries -BaseRegistry $baseVersionData.Registry -TargetRegistry $targetVersionData.Registry

    $comparisonRoot = Ensure-Directory -Path (Join-Path $OutputRoot (Join-Path 'comparisons' ("$CompareBaseVersion-to-$CompareTargetVersion")))
    $comparisonJsonPath = Join-Path $comparisonRoot 'comparison.json'
    $comparisonMarkdownPath = Join-Path $comparisonRoot 'comparison.md'
    $reviewTemplatePath = Join-Path $comparisonRoot 'update-review-template.json'

    $comparisonDocument = [pscustomobject]@{
        BaseVersion = $CompareBaseVersion
        TargetVersion = $CompareTargetVersion
        GeneratedAt = (Get-Date).ToString('s')
        EntryComparisons = @($entryComparisons)
    }

    $comparisonDocument | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $comparisonJsonPath -Encoding UTF8
    Export-ComparisonMarkdown -EntryComparisons $entryComparisons -BaseVersionName $CompareBaseVersion -TargetVersionName $CompareTargetVersion -MarkdownPath $comparisonMarkdownPath
    Export-UpdateReviewTemplate -EntryComparisons $entryComparisons -TemplatePath $reviewTemplatePath

    Write-Status "Comparison written to $comparisonJsonPath"
    Write-Status "Update review template written to $reviewTemplatePath"

    return [pscustomobject]@{
        ComparisonRoot = $comparisonRoot
        ComparisonJsonPath = $comparisonJsonPath
        ComparisonMarkdownPath = $comparisonMarkdownPath
        ReviewTemplatePath = $reviewTemplatePath
    }
}

switch ($Action) {
    'Collect' {
        Invoke-Collect -SelectedVersion $Version -SelectedModules (Normalize-StringList -Values $Modules) -IncludedConfigKeys (Normalize-StringList -Values $IncludeConfigKey) | Out-Null
    }
    'Compare' {
        Invoke-Compare -CompareBaseVersion $BaseVersion -CompareTargetVersion $TargetVersion | Out-Null
    }
}