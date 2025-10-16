#!/usr/bin/env node
/**
 * Markdown to Steam BBCode Converter
 * 将 README.md、README_en.md 和 README.combat.md 转换为 Steam 创意工坊的 BBCode 格式
 */

const fs = require('fs');
const path = require('path');

// 读取 Markdown 文件
function readMarkdownFile(filePath) {
    try {
        return fs.readFileSync(filePath, 'utf-8');
    } catch (error) {
        console.error(`错误：无法读取文件 ${filePath}`);
        console.error(error.message);
        process.exit(1);
    }
}

// 转换 Markdown 到 Steam BBCode
function convertToSteamBBCode(markdown) {
    let result = markdown;

    // 0. 统一行结束符为 \n (移除 \r)
    result = result.replace(/\r\n/g, '\n').replace(/\r/g, '\n');

    // 1. 转换 h1 标题 (# 标题)
    result = result.replace(/^# (.+)$/gm, '[h1]$1[/h1]');

    // 2. 转换 h2 标题 (## 标题) - 前面添加 hr
    result = result.replace(/^## (.+)$/gm, '\n\n[hr][/hr]\n[h2]$1[/h2]');

    // 3. 转换 h3 标题 (### 标题)
    result = result.replace(/^### (.+)$/gm, '[h3]$1[/h3]');

    // 4. 处理列表 - 需要在处理粗体之前处理，以便正确识别列表项
    const lines = result.split('\n');
    const processedLines = [];
    let inList = false;
    let inOList = false;

    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        const isListItem = /^[\-\*] (.+)$/.test(line);
        const isOListItem = /^\d+\. (.+)$/.test(line);

        // 处理无序列表
        if (isListItem) {
            if (!inList) {
                processedLines.push('[list]');
                inList = true;
            }
            // 保留列表项内容，稍后处理粗体
            processedLines.push(line.replace(/^[\-\*] /, '[*] '));
        } else if (inList && line.trim() === '') {
            // 空行不结束列表
            processedLines.push(line);
        } else if (inList && !isOListItem) {
            processedLines.push('[/list]');
            processedLines.push(line);
            inList = false;
        }
        // 处理有序列表
        else if (isOListItem) {
            if (!inOList) {
                processedLines.push('[olist]');
                inOList = true;
            }
            processedLines.push(line.replace(/^\d+\. /, '[*] '));
        } else if (inOList && line.trim() === '') {
            // 空行不结束列表
            processedLines.push(line);
        } else if (inOList && !isListItem) {
            processedLines.push('[/olist]');
            processedLines.push(line);
            inOList = false;
        }
        // 普通行
        else {
            processedLines.push(line);
        }
    }

    // 关闭未闭合的列表
    if (inList) processedLines.push('[/list]');
    if (inOList) processedLines.push('[/olist]');

    result = processedLines.join('\n');

    // 5. 转换粗体 (**文本** 或 __文本__)
    result = result.replace(/\*\*(.+?)\*\*/g, '[b]$1[/b]');
    result = result.replace(/__(.+?)__/g, '[b]$1[/b]');

    // 6. 转换斜体 (*文本* 或 _文本_) - 注意不要匹配列表标记 [*]
    result = result.replace(/(?<!\[)\*([^*\[\]]+)\*/g, '[i]$1[/i]');
    result = result.replace(/(?<!\[)_([^_\[\]]+)_/g, '[i]$1[/i]');

    // 7. 转换代码块 (```代码```)
    result = result.replace(/```[\s\S]*?```/g, (match) => {
        const code = match.replace(/```\w*\n?/, '').replace(/```$/, '');
        return `[code]${code}[/code]`;
    });

    // 8. 转换行内代码 (`代码`)
    result = result.replace(/`(.+?)`/g, '[code]$1[/code]');

    // 9. 转换链接 [文本](URL)
    result = result.replace(/\[(.+?)\]\((.+?)\)/g, '[url=$2]$1[/url]');

    // 10. 清理多余的空行（最多保留2个连续换行）
    result = result.replace(/\n{3,}/g, '\n\n');

    // 11. 移除第一行的 hr（如果 h1 前面有的话）
    result = result.replace(/^\[hr\]\[\/hr\]\n/, '');

    // 12. 清理开头和结尾的空白
    result = result.trim();

    return result;
}

// 保存转换后的文件
function saveFile(filePath, content) {
    try {
        fs.writeFileSync(filePath, content, 'utf-8');
        console.log(`✓ 成功生成 ${filePath}`);
    } catch (error) {
        console.error(`错误：无法保存文件 ${filePath}`);
        console.error(error.message);
        process.exit(1);
    }
}

// 主函数
function main() {
    // 如果指定了参数，则使用参数
    if (process.argv.length > 2) {
        const inputFile = process.argv[2];
        const outputFile = process.argv[3] || inputFile.replace('.md', '.steam.txt');
        convertSingleFile(inputFile, outputFile);
    } else {
        // 否则转换三个默认文件
        console.log('=== Markdown to Steam BBCode Converter ===');
        console.log('转换默认文件...\n');
        
        convertSingleFile('README.md', 'README.steam.txt');
        console.log('');
        convertSingleFile('README_en.md', 'README_en.steam.txt');
        console.log('');
        convertSingleFile('README.combat.md', 'README.combat.steam.txt');
        
        console.log('');
        console.log('=== 全部转换完成 ===');
    }
}

// 转换单个文件
function convertSingleFile(inputFile, outputFile) {
    console.log(`--- 转换 ${inputFile} ---`);
    console.log(`输出文件: ${outputFile}`);

    // 读取文件
    const markdown = readMarkdownFile(inputFile);
    console.log(`✓ 读取文件成功 (${markdown.length} 字符)`);

    // 转换格式
    const steamBBCode = convertToSteamBBCode(markdown);
    console.log(`✓ 转换完成 (${steamBBCode.length} 字符)`);

    // 保存文件
    saveFile(outputFile, steamBBCode);
}

// 如果直接运行此脚本
if (require.main === module) {
    main();
}

module.exports = { convertToSteamBBCode, readMarkdownFile, saveFile, convertSingleFile };
