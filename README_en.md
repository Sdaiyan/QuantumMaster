# Mandate of Heaven

The Taihu has mastered the Mandate of Heaven, capable of controlling various probability events in the game to achieve maximum benefits. This can greatly reduce the need for save-loading.

This MOD only does what can be done in the vanilla game. As long as the vanilla probability > 0.

For example:
In the wilderness, the vanilla game can only gather up to 3-quality primers, not 2-quality.
Combat ratings require certain positive actions to obtain combat reading/Zhoutian opportunities. Please check the wiki for details.


## Fortune Feature Description
The Mandate of Heaven have infinite applications. Under the reshaping of the world's will, you can now specify Taihu's current fortune. Different fortune levels have the following effects:

**Ill-Fated**: *Random rewards↓↓*
**Unlucky**: *Random rewards↓*
**Smooth Sailing**: *Vanilla experience, disables this feature*
**Well-Connected**: *Random rewards↑*
**Wish Come True**: *Random rewards↑↑*
**Lucky Star**: *Random rewards↑↑↑*
**Blessed from Heaven**: *Random rewards↑↑↑↑*
**Child of Destiny**: *All random rewards are at theoretical maximum (ineffective when probability is 0)*

## Global Fortune Description

Global fortune can **uniformly modify** options set to [Follow Global].

For example, if global fortune is set to [Child of Destiny] and Bluffing is set to [Follow Global], then when bluffing, the applicable fortune is [Child of Destiny].

For example, if global fortune is set to [Smooth Sailing] and Stealing is set to [Child of Destiny], then when stealing, the applicable fortune is [Child of Destiny].


## Feature List

Features with the [Fortune] prefix are affected by fortune.
In the following descriptions, if your fortune is negative, the effects are reversed.

## NPC Interactions
**[Fortune]Multiple Birth Probability**: *Probability↑*
**[Orientation]Sexual Orientation Control**: *Override everyone's sexual orientation*
**[Gender]Generated Gender Control**: *Can control the gender of NPCs that allow random gender*
**[Fortune]Steal/Rob/Cheat/Poison**: *Success rate↑ when Taihu is the initiator, success rate↓ when Taihu is the victim*

## World Generation
**[Resources]Tile Resources**: *When generating the world, tile resource limits and current values are maximized, affected by difficulty*
**[Fortune]Building Level and Quantity**: *Increased according to fortune during generation*
```
These features only take effect if enabled when creating the world
```

## Event Triggers
**[Fortune]Treasure Hunt**: *Probability↑*
**[Fortune]Rope Binding**: *Success rate↑*
**[Fortune]Caravan Robbery Probability**: *Reduce probability of caravans being robbed during movement*
**[Fortune]Caravan Income Critical Rate**: *Increase probability of caravans getting critical income*
**[Fortune]Divine Resonance**: *Probability↑*
**[Fortune]Combat Reading**: *Probability↑*
**[Fortune]Combat Loot Drop Rate**: *Probability↑ (vanilla limit is enemy count + 2)*
**[Fortune]Combat Zhoutian Circulation**: *Probability↑*
**[Fortune]Village Business Income**: *Success rate, gathering success rate, harvest quality↑*
**[Fortune]Village Income Quantity**: *Quantity of wood, metal, coins, prestige, etc.↑*
**[Fortune]Teaching Reading Zhoutian**: *Probability↑*
**[Fortune]Gambling Hall/Brothel Critical**: *Probability↑ (critical is 3x income)*
**[Fortune]Weapon Durability Consumption**: *Probability↓ (ineffective when forced deduction)*
**[Fortune]Armor Durability Consumption**: *Probability↓ (ineffective when forced deduction)*
**[Fortune]Villager Business Aptitude Improvement**: *Probability↑ (ineffective when potential is exhausted)*
**[Fortune]Adventure Resource Quantity**: *Quantity↑*
**[Fortune]Success Probability When Receiving Guidance**: *Probability↑ (not the probability of being guided, but the probability of successful learning after being guided)*

## Gathering
**[Fortune]Tile Gathering Quality**: *Quality↑*
**[Fortune]Tile Gathering Quantity**: *Quantity↑*
**[Fortune]Monthly Tile Resource Recovery**: *Recovery quantity↑*
**[Fortune]Careful Selection Quality Upgrade**: *Quality↑ (better results based on guaranteed logic)*
**[Fortune]Resource Building Heartwood Probability**: *Probability↑ (has built-in cooldown, 3 months for low quality, 6 months for high quality)*
**[Fortune]Careful Selection Monthly Guarantee Accumulation**: *Quality quantity↑ (better results based on guaranteed logic)*

## Books
**[Fortune]Flash of Inspiration**: *Probability↑*
**[Fortune]Reading Progress Strategy**: *Random progress increase↑*
**[Fortune]Reading Efficiency Strategy**: *Random efficiency increase↑*
**[Generate]Book Generation Parameters**: *When generating books, complete pages are at maximum of floating range, lost pages are at minimum of floating range*
**[Specify]Specify Reading Strategy**: *Can specify each reading strategy*
**[Generate]Complete Pages Fixed at Front**: *Requires book generation parameters feature enabled. When this feature is enabled, complete pages will appear in the front sections of books*

## Crickets
**[Durability]Healthy Crickets**: *When generating crickets, always generate crickets with maximum theoretical durability limit and no injuries*
**[Fortune]Catch Double Cricket Probability**: *Probability↑*
**[Fortune]Cricket Catching Base Success Rate**: *Probability↑*
**[Fortune]Cricket Divine Appearance Probability**: *Probability↑ (requires part matching and must be enabled during cricket generation, check WIKI for specific mechanics)*

## Durability
**[Fortune]Item Durability Limit**: *Limit value↑ (original logic is half of limit value - random up to limit value)*

## Skill Cultivation
**[Strategy]Specify Zhoutian Strategy**: *Can specify each Zhoutian strategy*
**[Fortune]Zhoutian Internal Power Gain**: *When circulating Zhoutian, internal power gained increases within floating range according to set fortune↑*
**[Fortune]Zhoutian Internal Power Strategy Max Benefit**: *When circulating Zhoutian, internal power gained through strategy increases according to fortune↑*

## Sect Visits
**[Visit]Sect Initial Support**: *When Taihu first sends a visiting card to a sect, they will gain support from some people by default. The specific people are random. When enabled, support from the highest quality people will be obtained. Theoretically, the support gained will be higher*

## Open Source
https://github.com/Sdaiyan/QuantumMaster/tree/reborn
Please comply with GPL-3.0 license
