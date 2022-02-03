using R2API;
using System;

namespace SniperClassic.Modules
{
    public static class Tokens
    {
        public static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SNIPERCLASSIC_BODY_NAME", "Sniper");
            LanguageAPI.Add("SNIPERCLASSIC_BODY_SUBTITLE", "Eagle Eye");
            LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR", "..and so they left, the sound still ringing in deaf ears.");
            LanguageAPI.Add("SNIPERCLASSIC_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR", "..and so they vanished, both missed by no one.");

            #region skills
            LanguageAPI.Add("SNIPERCLASSIC_PASSIVE_NAME", "Magnum Force");
            LanguageAPI.Add("SNIPERCLASSIC_PASSIVE_DESCRIPTION", "Bonuses to <style=cIsDamage>attack speed</style> increase the <style=cIsDamage>damage</style> of your <style=cIsUtility>primary</style> skill.");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_NAME", "Snipe");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>430% damage</style>. After firing, <style=cIsDamage>reload</style> to gain up to <style=cIsDamage>50%</style> extra damage if timed correctly.");

            LanguageAPI.Add("SNIPERCLASSIC_RELOAD_NAME", "Reload");
            LanguageAPI.Add("SNIPERCLASSIC_RELOAD_DESCRIPTION", "Reload your weapon.");


            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_NAME", "Mark");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>340% damage</style>. After emptying your clip, <style=cIsDamage>reload</style> and <style=cIsUtility>recharge your Spotter</style> if perfectly timed.");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_NAME", "Hard Impact");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION", "Fire a <style=cIsDamage>Mortar</style> projectile for <style=cIsDamage>500% damage</style>. After firing, <style=cIsDamage>reload</style> to gain up to <style=cIsDamage>50%</style> extra damage if timed correctly.");


            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_NAME", "Steady Aim");

            string secondaryDesc = "<style=cIsDamage>Stunning</style>. Carefully take aim, <style=cIsDamage>multiplying the damage</style> of your next shot by up to <style=cIsDamage>300%</style>.";
            if (EntityStates.SniperClassicSkills.SecondaryScope.useScrollWheelZoom)
            {
                secondaryDesc += " Scroll wheel changes zoom level.";
            }
            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_DESCRIPTION", secondaryDesc);

            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_NAME", "Combat Training");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_DESCRIPTION", "<style=cIsUtility>Roll</style> a short distance and <style=cIsDamage>reload</style> your weapon.");

            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_BACKFLIP_NAME", "Military Training");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION", "<style=cIsUtility>Backflip</style> into the air and <style=cIsDamage>reload</style> your weapon. <style=cIsDamage>Shock</style> enemies in front of you with your <style=cIsDamage>Spotter</style> if it is <style=cIsDamage>charged</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_SMOKE_NAME", "Smokescreen");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_SMOKE_DESCRIPTION", "Cover an area in smoke for 12 seconds, <style=cIsUtility>slowing</style> enemies and making all allies <style=cIsUtility>invisible</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_NAME", "Spotter: FEEDBACK");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style>. Hit them for <style=cIsDamage>over 1000% damage</style> to zap nearby enemies for <style=cIsDamage>60% damage</style>. Afterwards, <style=cIsHealth>your Spotter will need to recharge</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_SCEPTER_NAME", "Spotter: OVERLOAD");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_SCEPTER_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style>. Hit them for <style=cIsDamage>over 1000% damage</style> to zap nearby enemies for <style=cIsDamage>120% damage</style>. Afterwards, <style=cIsHealth>your Spotter will need to recharge</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_ALT_NAME", "Spotter: DISRUPT");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_ALT_DESCRIPTION", "<style=cIsDamage>Stunning</style>. <style=cIsDamage>Analyze an enemy</style> for 7 seconds, <style=cIsUtility>distracting</style> nearby enemies while dealing <style=cIsDamage>7x100% damage</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_NAME", "Spotter: OUTBURST");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_DESCRIPTION", "<style=cIsDamage>Shocking</style>. <style=cIsDamage>Analyze an enemy</style> for 7 seconds, <style=cIsUtility>distracting</style> nearby enemies while dealing <style=cIsDamage>7x200% damage</style>");

            LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_SPOTTER", "<style=cKeywordName>Spotter</style><style=cSub>This skill has an additional effect if your Spotter drone is <style=cIsDamage>charged</style>. <i>Recharge rate increases with attack speed.</i></style>");
            LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_ANALYZED", "<style=cKeywordName>Analyzed</style><style=cSub>Reduce movement speed by <style=cIsDamage>40%</style> and reduce armor by <style=cIsDamage>30</style>.</style>");
            LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_MORTAR", "<style=cKeywordName>Mortar</style><style=cSubAn explosive projectile that gains <style=cIsDamage>extra blast radius</style> with distance traveled.</style>");

            #endregion skills

            String sniperDesc = "";
            sniperDesc += "The Sniper is a long-ranged survivor who works with his trusty Spotter drone to deal massive damage to targets from afar.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Snipe must be reloaded after every shot. Learn the timing to maximize your damage output!" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Attack speed increases the size of Snipe's perfect reload window and boosts the recharge rate of your Spotter." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Military Training fires an additional attack from your Spotter if it is charged." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Steady Aim combined with Spotter: FEEDBACK and a perfectly reloaded Snipe can wipe out crowds of enemies." + Environment.NewLine + Environment.NewLine;
            LanguageAPI.Add("SNIPERCLASSIC_DESCRIPTION", sniperDesc);

            String tldr = "bio: sniper was born with a special power he was stronger than all his crewmates at the ues contact light. he served in the risk of rain military fighting providence and in the final battel against providence they were fighting and providence turned him to the darkness and sniper turned against HAN-D and kill him preventing him from being in the sequel. he killed his own spotter drone in the battle which is why it isn't here please stop PMing asking me why that's why. also capes are cool fuck you space_cowboy226 everyone knows youre a red item stealing faggot\n\n<style=cIsHealing>likes: snipin, bein badass (gearbox style), crowbars, the reaper (from deadbolt), killing, death, dubstep, backflips, razor chroma key, hot huntresses with big boobys who dress like sluts, decoys, the reaper (from real life), railguns, capes (the cool kind not the gay kind)</style>\n\n<style=cIsHealth>dislikes: spotter drones, wisps, frenzied elites, the enforcer from ues fuck you enforcer stop showin everyone my deviantart logs you peace of shit, mithrix, desk plants, space_cowboy226 (mega ass-faggot), rain, life, the captain, overloading worms</style>\n\n<color=#FF0000>@risk_of_rainin_blood</color>";
            LanguageAPI.Add("SNIPERCLASSIC_BODY_LORE", tldr);

            #region skins and achievements
            LanguageAPI.Add("SNIPERCLASSIC_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("SNIPERCLASSIC_MASTERY_SKIN_NAME", "Operative");
            LanguageAPI.Add("SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME", "Expedition");

            //character
            LanguageAPI.Add("SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_NAME", "Riot");
            LanguageAPI.Add("SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_DESC", "Kill a Magma Worm, a Wandering Vagrant and a Stone Titan in a single run.");
            LanguageAPI.Add("SNIPERCLASSIC_CHARACTERUNLOCKABLE_UNLOCKABLE_NAME", "Riot");

            LanguageAPI.Add("SNIPERCLASSIC_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Sniper: Mastery");
            LanguageAPI.Add("SNIPERCLASSIC_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Sniper, beat the game or obliterate on Monsoon.");
            LanguageAPI.Add("SNIPERCLASSIC_MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Sniper: Mastery");

            string masteryFootnote = SniperClassic.starstormInstalled ? "" : "\n<color=#8888>(Typhoon difficulty requires Starstorm 2)</color>";

            LanguageAPI.Add("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Sniper: Grand Mastery");
            LanguageAPI.Add("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Sniper, beat the game or obliterate on Typhoon or higher." + masteryFootnote);
            LanguageAPI.Add("SNIPERCLASSIC_GRANDMASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Sniper: Grand Mastery");

            #endregion skins and achievements
        }
    }
}