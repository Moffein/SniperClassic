using RoR2.Skills;
using R2API;
using RoR2.UI;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using EntityStates;
using EntityStates.SniperClassicSkills;
using EntityStates.SniperClassicSkills.Nemesis;

namespace SniperClassic
{
    public static class Nemesis
    {
        public static SkillDef specialSpotDef, specialSpotDisruptDef;

        //A mashup of the various nemesis assets can be found here.
        //Run this after Sniper finishes setup, since Sniper's specials are reused.
            //Might need to remake skilldefs later anyways if they're gonna have different unlock requirements on each character.
        public static void Setup()
        {
            //Components needed by NemSniper body: RailgunHeatController, SpotterTargetingController, SpotterLightningController
            //Copypaste the dronelauncher state machine from SniperClassic.cs and add it to NemSniper
            NemesisCrosshairSetup();    //Move this to the place where nemsniper's stats are set.
            AssignSkills();
            BuildEffects();
        }

        private static GameObject NemesisCrosshairSetup()
        {
            //Todo: Figure out how to get this to show spread for better feedback on M2 shots.
            GameObject crosshair = SniperContent.assetBundle.LoadAsset<GameObject>("NoscopeCrosshair.prefab").InstantiateClone("MoffeinSniperClassicNemesisCrosshair", false);
            crosshair.AddComponent<HudElement>();
            CrosshairController cc = crosshair.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            crosshair.AddComponent<RailgunHeatIndicatorController>();

            return crosshair;
        }
        
        //These need to actually be assigned to a body.
        private static void AssignSkills()
        {
            RegisterLanguageTokens();
            AssignPrimaries();
            AssignSecondaries();
            AssignUtilities();
            //Special skilldefs can just re-use everything from regular Sniper. No need to add anything to the contentpack.
        }

        private static void AssignPrimaries()
        {
            SkillDef primaryTakeOutDef = SkillDef.CreateInstance<SkillDef>();
            primaryTakeOutDef.activationState = new SerializableEntityStateType(typeof(FireRailgunSingle));
            primaryTakeOutDef.activationStateMachineName = "Weapon";
            primaryTakeOutDef.baseMaxStock = 1;
            primaryTakeOutDef.baseRechargeInterval = 0f;
            primaryTakeOutDef.beginSkillCooldownOnSkillEnd = false;
            primaryTakeOutDef.canceledFromSprinting = false;
            primaryTakeOutDef.dontAllowPastMaxStocks = true;
            primaryTakeOutDef.forceSprintDuringState = false;
            primaryTakeOutDef.fullRestockOnAssign = true;
            primaryTakeOutDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryIcon.png");
            primaryTakeOutDef.interruptPriority = InterruptPriority.Any;
            primaryTakeOutDef.isCombatSkill = true;
            primaryTakeOutDef.keywordTokens = new string[] { };
            primaryTakeOutDef.mustKeyPress = false;
            primaryTakeOutDef.cancelSprintingOnActivation = true;
            primaryTakeOutDef.rechargeStock = 1;
            primaryTakeOutDef.requiredStock = 1;
            primaryTakeOutDef.skillName = "FireRailgunSingle";
            primaryTakeOutDef.skillNameToken = "SNIPERCLASSICNEMESIS_PRIMARY_NAME";
            primaryTakeOutDef.skillDescriptionToken = "SNIPERCLASSICNEMESIS_PRIMARY_DESCRIPTION";
            primaryTakeOutDef.stockToConsume = 1;
            SniperContent.entityStates.Add(typeof(FireRailgunSingle));
            SniperContent.skillDefs.Add(primaryTakeOutDef);
        }
        private static void AssignSecondaries()
        {
            SkillDef secondaryDischargeDef = SkillDef.CreateInstance<SkillDef>();
            secondaryDischargeDef.activationState = new SerializableEntityStateType(typeof(FireRailgunSingle));
            secondaryDischargeDef.activationStateMachineName = "Weapon";
            secondaryDischargeDef.baseMaxStock = 1;
            secondaryDischargeDef.baseRechargeInterval = 6f;
            secondaryDischargeDef.beginSkillCooldownOnSkillEnd = true;
            secondaryDischargeDef.canceledFromSprinting = false;
            secondaryDischargeDef.dontAllowPastMaxStocks = true;
            secondaryDischargeDef.forceSprintDuringState = false;
            secondaryDischargeDef.fullRestockOnAssign = true;
            secondaryDischargeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryIcon.png");
            secondaryDischargeDef.interruptPriority = InterruptPriority.Skill;
            secondaryDischargeDef.isCombatSkill = true;
            secondaryDischargeDef.keywordTokens = new string[] { };
            secondaryDischargeDef.mustKeyPress = false;
            secondaryDischargeDef.cancelSprintingOnActivation = true;
            secondaryDischargeDef.rechargeStock = 1;
            secondaryDischargeDef.requiredStock = 1;
            secondaryDischargeDef.skillName = "DischargeRailgunSingle";
            secondaryDischargeDef.skillNameToken = "SNIPERCLASSICNEMESIS_SECONDARY_NAME";   //Rename to Sudden Discharge, planned rapid hit alt will fit the "Steady Discharge" name.
            secondaryDischargeDef.skillDescriptionToken = "SNIPERCLASSICNEMESIS_SECONDARY_DESCRIPTION";
            secondaryDischargeDef.stockToConsume = 1;
            SniperContent.entityStates.Add(typeof(FireRailgunSingle));
            SniperContent.skillDefs.Add(secondaryDischargeDef);
        }

        //Set NemSniper's default utility to Roll, and default special to DISRUPT.
        //Modify Utility skilldefs. Remove mentions of reloading in the description, use different nemesis-colored icons?
        //Possibility of utilities removing the Overheat status? Would be easy to implement, but need to see how it plays.
        private static void AssignUtilities()
        {
            SkillDef utilityRollDef = SkillDef.CreateInstance<SkillDef>();
            utilityRollDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.CombatRoll));
            utilityRollDef.activationStateMachineName = "Body";
            utilityRollDef.baseMaxStock = 1;
            utilityRollDef.baseRechargeInterval = 4f;
            utilityRollDef.beginSkillCooldownOnSkillEnd = false;
            utilityRollDef.canceledFromSprinting = false;
            utilityRollDef.dontAllowPastMaxStocks = true;
            utilityRollDef.forceSprintDuringState = false;
            utilityRollDef.fullRestockOnAssign = true;
            utilityRollDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilityAltIcon.png");
            utilityRollDef.interruptPriority = InterruptPriority.Any;
            utilityRollDef.isCombatSkill = false;
            utilityRollDef.keywordTokens = new string[] {};
            utilityRollDef.mustKeyPress = false;
            utilityRollDef.cancelSprintingOnActivation = false;
            utilityRollDef.rechargeStock = 1;
            utilityRollDef.requiredStock = 1;
            utilityRollDef.skillName = "CombatRoll";
            utilityRollDef.skillNameToken = "SNIPERCLASSICNEMESIS_UTILITY_NAME";
            utilityRollDef.skillDescriptionToken = "SNIPERCLASSICNEMESIS_UTILITY_DESCRIPTION";
            utilityRollDef.stockToConsume = 1;
            //No need to re-add the entitystate. Skilldef is different so it probably should be re-added.
            SniperContent.skillDefs.Add(utilityRollDef);

            SkillDef utilityBackflipDef = SkillDef.CreateInstance<SkillDef>();
            utilityBackflipDef.activationState = new SerializableEntityStateType(typeof(Backflip));
            utilityBackflipDef.activationStateMachineName = "Body";
            utilityBackflipDef.baseMaxStock = 1;
            utilityBackflipDef.baseRechargeInterval = 6f;
            utilityBackflipDef.beginSkillCooldownOnSkillEnd = false;
            utilityBackflipDef.canceledFromSprinting = false;
            utilityBackflipDef.dontAllowPastMaxStocks = true;
            utilityBackflipDef.forceSprintDuringState = false;
            utilityBackflipDef.fullRestockOnAssign = true;
            utilityBackflipDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilityIcon.png");
            utilityBackflipDef.interruptPriority = InterruptPriority.Any;
            utilityBackflipDef.isCombatSkill = false;
            utilityBackflipDef.keywordTokens = new string[] {};
            utilityBackflipDef.mustKeyPress = false;
            utilityBackflipDef.cancelSprintingOnActivation = false;
            utilityBackflipDef.rechargeStock = 1;
            utilityBackflipDef.requiredStock = 1;
            utilityBackflipDef.skillName = "CombatBackflip";
            utilityBackflipDef.skillNameToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_NAME";
            utilityBackflipDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION";
            utilityBackflipDef.stockToConsume = 1;
            //No need to re-add the entitystate. Skilldef is different so it probably should be re-added.
            SniperContent.skillDefs.Add(utilityBackflipDef);
        }

        private static void RegisterLanguageTokens()
        {
            /*R2API.LanguageAPI.Add("SNIPERCLASSICNEMESIS_BODY_NAME", "Nemesis Sniper");
            R2API.LanguageAPI.Add("SNIPERCLASSICNEMESIS_BODY_SUBTITLE", "Vigilant Warden");
            R2API.LanguageAPI.Add("SNIPERCLASSICNEMESIS_DEFAULT_SKIN_NAME", "Default");*/
        }

        private static void BuildEffects()
        {
            //TODO: FireRailGunSingle - Tracer and Hit effects
            //TODO: DischargeRailGunSingle - Tracer and Hit effects
        }
    }
}
