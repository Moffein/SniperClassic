using UnityEngine;
using R2API;
using RoR2;
using RoR2.Skills;
using SniperClassic.Modules;
using System.Runtime.CompilerServices;
using System;
using EntityStates.SniperClassicSkills;
using EntityStates;
using RoR2.UI;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SniperClassic.Setup
{
    public static class BuildSkills
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            SkillLocator sk = SniperClassic.SniperBody.GetComponent<SkillLocator>();
            if (sk)
            {
                /*if (SniperClassic.enableWeakPoints)
                {
                    sk.passiveSkill.enabled = true;
                    sk.passiveSkill.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPassive.png");
                    sk.passiveSkill.skillNameToken = "SNIPERCLASSIC_PASSIVE_NAME";
                    sk.passiveSkill.skillDescriptionToken = "SNIPERCLASSIC_PASSIVE_DESCRIPTION";
                }*/
                AssignPrimary(sk);
                AssignSecondary(sk);
                AssignUtility(sk);
                AssignSpecial(sk);
            }

            if (SniperClassic.scepterInstalled) SetupScepterSkills();
            if (SniperClassic.classicItemsInstalled) SetupScepterClassicSkills();
        }
        private static void AssignPrimary(SkillLocator sk)
        {

            //SniperContent.entityStates.Add(typeof(BaseSnipeState));
            //SniperContent.entityStates.Add(typeof(BaseReloadState));

            SniperContent.entityStates.Add(typeof(AIReload));

            Sprite iconReload = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryReloadIcon.png");

            SkillFamily primarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (primarySkillFamily as ScriptableObject).name = "primary";
            primarySkillFamily.defaultVariantIndex = 0u;
            primarySkillFamily.variants = new SkillFamily.Variant[1];
            sk.primary._skillFamily = primarySkillFamily;

            SkillDef primarySnipeDef = SkillDef.CreateInstance<SkillDef>();
            primarySnipeDef.activationState = new SerializableEntityStateType(typeof(Snipe));
            primarySnipeDef.activationStateMachineName = "Weapon";
            primarySnipeDef.baseMaxStock = 1;
            primarySnipeDef.baseRechargeInterval = 0f;
            primarySnipeDef.beginSkillCooldownOnSkillEnd = false;
            primarySnipeDef.canceledFromSprinting = false;
            primarySnipeDef.dontAllowPastMaxStocks = true;
            primarySnipeDef.forceSprintDuringState = false;
            primarySnipeDef.fullRestockOnAssign = true;
            primarySnipeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryIcon.png");
            primarySnipeDef.interruptPriority = InterruptPriority.Any;
            primarySnipeDef.isCombatSkill = true;
            primarySnipeDef.keywordTokens = new string[] { };
            primarySnipeDef.mustKeyPress = false;
            primarySnipeDef.cancelSprintingOnActivation = true;
            primarySnipeDef.rechargeStock = 1;
            primarySnipeDef.requiredStock = 1;
            primarySnipeDef.skillName = "Snipe";
            primarySnipeDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_NAME";
            primarySnipeDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_DESCRIPTION";
            primarySnipeDef.stockToConsume = 1;
            FixScriptableObjectName(primarySnipeDef);

            SkillDef primarySnipeReloadDef = SkillDef.CreateInstance<SkillDef>();
            primarySnipeReloadDef.activationState = new SerializableEntityStateType(typeof(ReloadSnipe));
            primarySnipeReloadDef.activationStateMachineName = "Weapon";
            primarySnipeReloadDef.baseMaxStock = 1;
            primarySnipeReloadDef.baseRechargeInterval = 0f;
            primarySnipeReloadDef.beginSkillCooldownOnSkillEnd = false;
            primarySnipeReloadDef.canceledFromSprinting = false;
            primarySnipeReloadDef.dontAllowPastMaxStocks = true;
            primarySnipeReloadDef.forceSprintDuringState = false;
            primarySnipeReloadDef.fullRestockOnAssign = true;
            primarySnipeReloadDef.icon = iconReload;
            primarySnipeReloadDef.interruptPriority = InterruptPriority.Skill;
            primarySnipeReloadDef.isCombatSkill = true;
            primarySnipeReloadDef.keywordTokens = new string[] { };
            primarySnipeReloadDef.mustKeyPress = true;
            primarySnipeReloadDef.cancelSprintingOnActivation = false;
            primarySnipeReloadDef.rechargeStock = 0;
            primarySnipeReloadDef.requiredStock = 1;
            primarySnipeReloadDef.skillName = "ReloadSnipe";
            primarySnipeReloadDef.skillNameToken = "SNIPERCLASSIC_RELOAD_NAME";
            primarySnipeReloadDef.skillDescriptionToken = "SNIPERCLASSIC_RELOAD_DESCRIPTION";
            primarySnipeReloadDef.stockToConsume = 1;
            FixScriptableObjectName(primarySnipeReloadDef);

            Snipe.reloadDef = primarySnipeReloadDef;

            SniperContent.entityStates.Add(typeof(Snipe));
            SniperContent.entityStates.Add(typeof(ReloadSnipe));
            SniperContent.skillDefs.Add(primarySnipeDef);
            SniperContent.skillDefs.Add(primarySnipeReloadDef);
            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primarySnipeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySnipeDef.skillNameToken, false)
            };
            SniperContent.Skills.Primary.Snipe = primarySnipeDef;

            SkillDef primaryBRReloadDef = SkillDef.CreateInstance<SkillDef>();
            primaryBRReloadDef.activationState = new SerializableEntityStateType(typeof(ReloadBR));
            primaryBRReloadDef.activationStateMachineName = "Weapon";
            primaryBRReloadDef.baseMaxStock = 1;
            primaryBRReloadDef.baseRechargeInterval = 0f;
            primaryBRReloadDef.beginSkillCooldownOnSkillEnd = false;
            primaryBRReloadDef.canceledFromSprinting = false;
            primaryBRReloadDef.dontAllowPastMaxStocks = true;
            primaryBRReloadDef.forceSprintDuringState = false;
            primaryBRReloadDef.fullRestockOnAssign = true;
            primaryBRReloadDef.icon = iconReload;
            primaryBRReloadDef.interruptPriority = InterruptPriority.Skill;
            primaryBRReloadDef.isCombatSkill = true;
            primaryBRReloadDef.keywordTokens = new string[] { };
            primaryBRReloadDef.mustKeyPress = true;
            primaryBRReloadDef.cancelSprintingOnActivation = false;
            primaryBRReloadDef.rechargeStock = 0;
            primaryBRReloadDef.requiredStock = 1;
            primaryBRReloadDef.skillName = "ReloadBR";
            primaryBRReloadDef.skillNameToken = "SNIPERCLASSIC_RELOAD_NAME";
            primaryBRReloadDef.skillDescriptionToken = "SNIPERCLASSIC_RELOAD_DESCRIPTION";
            primaryBRReloadDef.stockToConsume = 1;
            FixScriptableObjectName(primaryBRReloadDef);
            FireBattleRifle.reloadDef = primaryBRReloadDef;
            SniperContent.skillDefs.Add(primaryBRReloadDef);

            SkillDef primaryBRDef = SkillDef.CreateInstance<SkillDef>();
            primaryBRDef.activationState = new SerializableEntityStateType(typeof(FireBattleRifle));
            primaryBRDef.activationStateMachineName = "Weapon";
            primaryBRDef.baseMaxStock = 5;
            primaryBRDef.baseRechargeInterval = 0f;
            primaryBRDef.beginSkillCooldownOnSkillEnd = false;
            primaryBRDef.canceledFromSprinting = false;
            primaryBRDef.dontAllowPastMaxStocks = true;
            primaryBRDef.forceSprintDuringState = false;
            primaryBRDef.fullRestockOnAssign = true;
            primaryBRDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryAltIcon.png");
            primaryBRDef.interruptPriority = InterruptPriority.Any;
            primaryBRDef.isCombatSkill = true;
            primaryBRDef.keywordTokens = new string[] { };
            primaryBRDef.mustKeyPress = false;
            primaryBRDef.cancelSprintingOnActivation = true;
            primaryBRDef.rechargeStock = 0;
            primaryBRDef.requiredStock = 1;
            primaryBRDef.skillName = "BattleRifle";
            primaryBRDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT_NAME";
            primaryBRDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION";
            primaryBRDef.stockToConsume = 1;
            FixScriptableObjectName(primaryBRDef);
            SniperContent.skillDefs.Add(primaryBRDef);
            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primaryBRDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryBRDef.skillNameToken, false)
            };
            SniperContent.entityStates.Add(typeof(FireBattleRifle));
            SniperContent.entityStates.Add(typeof(ReloadBR));
            SniperContent.Skills.Primary.Mark = primaryBRDef;

            SkillDef primaryHeavySnipeDef = SkillDef.CreateInstance<SkillDef>();
            primaryHeavySnipeDef.activationState = new SerializableEntityStateType(typeof(HeavySnipe));
            primaryHeavySnipeDef.activationStateMachineName = "Weapon";
            primaryHeavySnipeDef.baseMaxStock = 1;
            primaryHeavySnipeDef.baseRechargeInterval = 0f;
            primaryHeavySnipeDef.beginSkillCooldownOnSkillEnd = false;
            primaryHeavySnipeDef.canceledFromSprinting = false;
            primaryHeavySnipeDef.dontAllowPastMaxStocks = true;
            primaryHeavySnipeDef.forceSprintDuringState = false;
            primaryHeavySnipeDef.fullRestockOnAssign = true;
            primaryHeavySnipeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryAlt2Icon.png");
            primaryHeavySnipeDef.interruptPriority = InterruptPriority.Any;
            primaryHeavySnipeDef.isCombatSkill = true;
            primaryHeavySnipeDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_MORTAR" };
            primaryHeavySnipeDef.mustKeyPress = false;
            primaryHeavySnipeDef.cancelSprintingOnActivation = true;
            primaryHeavySnipeDef.rechargeStock = 1;
            primaryHeavySnipeDef.requiredStock = 1;
            primaryHeavySnipeDef.skillName = "HeavySnipe";
            primaryHeavySnipeDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT2_NAME";
            primaryHeavySnipeDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION";
            primaryHeavySnipeDef.stockToConsume = 1;
            FixScriptableObjectName(primaryHeavySnipeDef);
            SniperContent.entityStates.Add(typeof(HeavySnipe));
            SniperContent.skillDefs.Add(primaryHeavySnipeDef);

            SkillDef primaryHeavySnipeReloadDef = SkillDef.CreateInstance<SkillDef>();
            primaryHeavySnipeReloadDef.activationState = new SerializableEntityStateType(typeof(ReloadHeavySnipe));
            primaryHeavySnipeReloadDef.activationStateMachineName = "Weapon";
            primaryHeavySnipeReloadDef.baseMaxStock = 1;
            primaryHeavySnipeReloadDef.baseRechargeInterval = 0f;
            primaryHeavySnipeReloadDef.beginSkillCooldownOnSkillEnd = false;
            primaryHeavySnipeReloadDef.canceledFromSprinting = false;
            primaryHeavySnipeReloadDef.dontAllowPastMaxStocks = true;
            primaryHeavySnipeReloadDef.forceSprintDuringState = false;
            primaryHeavySnipeReloadDef.fullRestockOnAssign = true;
            primaryHeavySnipeReloadDef.icon = iconReload;
            primaryHeavySnipeReloadDef.interruptPriority = InterruptPriority.Skill;
            primaryHeavySnipeReloadDef.isCombatSkill = true;
            primaryHeavySnipeReloadDef.keywordTokens = new string[] { };
            primaryHeavySnipeReloadDef.mustKeyPress = true;
            primaryHeavySnipeReloadDef.cancelSprintingOnActivation = false;
            primaryHeavySnipeReloadDef.rechargeStock = 0;
            primaryHeavySnipeReloadDef.requiredStock = 1;
            primaryHeavySnipeReloadDef.skillName = "ReloadHeavySnipe";
            primaryHeavySnipeReloadDef.skillNameToken = "SNIPERCLASSIC_RELOAD_NAME";
            primaryHeavySnipeReloadDef.skillDescriptionToken = "SNIPERCLASSIC_RELOAD_DESCRIPTION";
            primaryHeavySnipeReloadDef.stockToConsume = 1;
            FixScriptableObjectName(primaryHeavySnipeReloadDef);
            HeavySnipe.reloadDef = primaryHeavySnipeReloadDef;
            SniperContent.entityStates.Add(typeof(ReloadHeavySnipe));
            SniperContent.skillDefs.Add(primaryHeavySnipeReloadDef);

            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primaryHeavySnipeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryHeavySnipeDef.skillNameToken, false)
            };
            SniperContent.Skills.Primary.HardImpact = primaryHeavySnipeDef;

            SniperContent.skillFamilies.Add(primarySkillFamily);

            CharacterSelectSurvivorPreviewDisplayController previewController = SniperClassic.SniperDisplay.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            previewController.skillChangeResponses[0].triggerSkillFamily = primarySkillFamily;
            previewController.skillChangeResponses[0].triggerSkill = primarySnipeDef;
            previewController.skillChangeResponses[1].triggerSkillFamily = primarySkillFamily;
            previewController.skillChangeResponses[1].triggerSkill = primaryBRDef;
            previewController.skillChangeResponses[2].triggerSkillFamily = primarySkillFamily;
            previewController.skillChangeResponses[2].triggerSkill = primaryHeavySnipeDef;
        }

        private static void AssignSecondary(SkillLocator sk)
        {
            ScopeCrosshairSetup();
            ScopeStateMachineSetup();

            SkillFamily secondarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (secondarySkillFamily as ScriptableObject).name = "secondary";
            secondarySkillFamily.defaultVariantIndex = 0u;
            secondarySkillFamily.variants = new SkillFamily.Variant[1];
            sk.secondary._skillFamily = secondarySkillFamily;

            SkillDef secondaryScopeDef = SkillDef.CreateInstance<SkillDef>();
            secondaryScopeDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SecondaryScope));
            secondaryScopeDef.activationStateMachineName = "Scope";
            secondaryScopeDef.baseMaxStock = 1;
            secondaryScopeDef.baseRechargeInterval = 0f;
            secondaryScopeDef.beginSkillCooldownOnSkillEnd = false;
            secondaryScopeDef.canceledFromSprinting = false;
            secondaryScopeDef.dontAllowPastMaxStocks = true;
            secondaryScopeDef.forceSprintDuringState = false;
            secondaryScopeDef.fullRestockOnAssign = true;
            secondaryScopeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSecondaryIcon.png");
            secondaryScopeDef.interruptPriority = InterruptPriority.Any;
            secondaryScopeDef.isCombatSkill = false;
            secondaryScopeDef.autoHandleLuminousShot = false;

            secondaryScopeDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_WEAKPOINT", "KEYWORD_SNIPERCLASSIC_OVERCHARGE" };
            secondaryScopeDef.skillDescriptionToken = "SNIPERCLASSIC_SECONDARY_DESCRIPTION";

            secondaryScopeDef.mustKeyPress = true;
            secondaryScopeDef.cancelSprintingOnActivation = true;
            secondaryScopeDef.rechargeStock = 1;
            secondaryScopeDef.requiredStock = 0;
            secondaryScopeDef.skillName = "EnterScope";
            secondaryScopeDef.skillNameToken = "SNIPERCLASSIC_SECONDARY_NAME";
            secondaryScopeDef.stockToConsume = 0;
            FixScriptableObjectName(secondaryScopeDef);
            SniperContent.entityStates.Add(typeof(SecondaryScope));
            SniperContent.skillDefs.Add(secondaryScopeDef);
            SniperContent.skillFamilies.Add(secondarySkillFamily);
            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondaryScopeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondaryScopeDef.skillNameToken, false)
            };

            SniperContent.Skills.Secondary.SteadyAim = secondaryScopeDef;
        }

        private static void ScopeCrosshairSetup()
        {
            GameObject visualizer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerSniperTargetVisualizerLight.prefab").WaitForCompletion().InstantiateClone("SniperClassicTargetVisualizer", false);
            visualizer.transform.localScale = 12f * Vector3.one;

            visualizer.transform.Find("Scaler/Rectangle").GetComponent<Image>().sprite = SniperContent.assetBundle.LoadAsset<Sprite>("texWeakpointVisualizer.png");
            visualizer.transform.Find("Scaler/Rectangle").GetComponent<Image>().color = Color.white;
            visualizer.transform.Find("Scaler/Outer").GetComponent<Image>().color = new Color(65f / 255f, 26f / 255f, 85f / 255f, 134f / 255f);

            GameObject visualizerGrayed = visualizer.InstantiateClone("SniperClassicTargetVisualizerGrayed", false);
            visualizerGrayed.transform.Find("Scaler/Rectangle").GetComponent<Image>().sprite = SniperContent.assetBundle.LoadAsset<Sprite>("texWeakpointVisualizerGrayed.png");
            visualizerGrayed.transform.Find("Scaler/Rectangle").GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            visualizerGrayed.transform.Find("Scaler/Outer").GetComponent<Image>().color = new Color(53f / 255f, 27f / 255f, 66f / 255f, 85f / 255f); new Color(0.5f, 0.5f, 0.5f, 0.5f);


            SecondaryScope.scopeCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("ScopeCrosshair.prefab").InstantiateClone("MoffeinSniperClassicScopeCrosshair", false);
            SecondaryScope.scopeCrosshairPrefab.AddComponent<HudElement>();
            CrosshairController cc = SecondaryScope.scopeCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.scopeCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();
            AddWeakpointUI(SecondaryScope.scopeCrosshairPrefab, visualizerGrayed);

            SecondaryScope.noscopeCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("NoscopeCrosshair.prefab").InstantiateClone("MoffeinSniperClassicNoscopeCrosshair", false);
            SecondaryScope.noscopeCrosshairPrefab.AddComponent<HudElement>();
            cc = SecondaryScope.noscopeCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.noscopeCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();
            AddWeakpointUI(SecondaryScope.noscopeCrosshairPrefab, visualizerGrayed);

            SecondaryScope.noscopeWeakpointCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("NoscopeCrosshair.prefab").InstantiateClone("MoffeinSniperClassicNoscopeWeakpointCrosshair", false);
            SecondaryScope.noscopeWeakpointCrosshairPrefab.AddComponent<HudElement>();
            cc = SecondaryScope.noscopeWeakpointCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.noscopeWeakpointCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();
            AddWeakpointUI(SecondaryScope.noscopeWeakpointCrosshairPrefab, visualizer);

            SecondaryScope.scopeWeakpointCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("ScopeCrosshair.prefab").InstantiateClone("MoffeinSniperClassicScopeWeakpointCrosshair", false);
            SecondaryScope.scopeWeakpointCrosshairPrefab.AddComponent<HudElement>();
            cc = SecondaryScope.scopeWeakpointCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.scopeWeakpointCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();
            AddWeakpointUI(SecondaryScope.scopeWeakpointCrosshairPrefab, visualizer);
        }

        private static void AddWeakpointUI(GameObject crosshair, GameObject visualizerPrefab)
        {
            if (!SniperClassic.enableWeakPoints) return;
            PointViewer pv = crosshair.AddComponent<PointViewer>();
            SniperTargetViewer stv = crosshair.AddComponent<SniperTargetViewer>();
            stv.visualizerPrefab = visualizerPrefab;
        }

        private static void ScopeStateMachineSetup()
        {
            EntityStateMachine scopeMachine = SniperClassic.SniperBody.AddComponent<EntityStateMachine>();
            scopeMachine.customName = "Scope";
            scopeMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));
            scopeMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));

            NetworkStateMachine nsm = SniperClassic.SniperBody.GetComponent<NetworkStateMachine>();
            nsm.stateMachines = nsm.stateMachines.Append(scopeMachine).ToArray();
        }


        private static void AssignUtility(SkillLocator sk)
        {
            SkillFamily utilitySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (utilitySkillFamily as ScriptableObject).name = "utility";
            utilitySkillFamily.defaultVariantIndex = 0u;
            utilitySkillFamily.variants = new SkillFamily.Variant[1];
            sk.utility._skillFamily = utilitySkillFamily;

            SkillDef utilityBackflipDef = SkillDef.CreateInstance<SkillDef>();
            utilityBackflipDef.activationState = new SerializableEntityStateType(typeof(Backflip));
            utilityBackflipDef.activationStateMachineName = "Body";
            utilityBackflipDef.baseMaxStock = 1;
            utilityBackflipDef.baseRechargeInterval = 6f;
            utilityBackflipDef.beginSkillCooldownOnSkillEnd = false;
            utilityBackflipDef.canceledFromSprinting = false;
            utilityBackflipDef.dontAllowPastMaxStocks = true;
            utilityBackflipDef.forceSprintDuringState = true;
            utilityBackflipDef.fullRestockOnAssign = true;
            utilityBackflipDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilityIcon.png");
            utilityBackflipDef.interruptPriority = InterruptPriority.Any;
            utilityBackflipDef.isCombatSkill = false;
            utilityBackflipDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            utilityBackflipDef.mustKeyPress = false;
            utilityBackflipDef.cancelSprintingOnActivation = false;
            utilityBackflipDef.rechargeStock = 1;
            utilityBackflipDef.requiredStock = 1;
            utilityBackflipDef.skillName = "CombatBackflip";
            utilityBackflipDef.skillNameToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_NAME";
            utilityBackflipDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION";
            utilityBackflipDef.stockToConsume = 1;
            FixScriptableObjectName(utilityBackflipDef);
            SniperContent.entityStates.Add(typeof(Backflip));
            SniperContent.skillDefs.Add(utilityBackflipDef);
            SniperContent.skillFamilies.Add(utilitySkillFamily);
            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = utilityBackflipDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityBackflipDef.skillNameToken, false)
            };
            SniperContent.Skills.Utility.Backflip = utilityBackflipDef;

            SkillDef utilityRollDef = SkillDef.CreateInstance<SkillDef>();
            utilityRollDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.CombatRoll));
            utilityRollDef.activationStateMachineName = "Body";
            utilityRollDef.baseMaxStock = 1;
            utilityRollDef.baseRechargeInterval = 6f;
            utilityRollDef.beginSkillCooldownOnSkillEnd = false;
            utilityRollDef.canceledFromSprinting = false;
            utilityRollDef.dontAllowPastMaxStocks = true;
            utilityRollDef.forceSprintDuringState = true;
            utilityRollDef.fullRestockOnAssign = true;
            utilityRollDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilityAltIcon.png");
            utilityRollDef.interruptPriority = InterruptPriority.Any;
            utilityRollDef.isCombatSkill = false;
            utilityRollDef.keywordTokens = new string[] { };
            utilityRollDef.mustKeyPress = false;
            utilityRollDef.cancelSprintingOnActivation = false;
            utilityRollDef.rechargeStock = 1;
            utilityRollDef.requiredStock = 1;
            utilityRollDef.skillName = "CombatRoll";
            utilityRollDef.skillNameToken = "SNIPERCLASSIC_UTILITY_NAME";
            utilityRollDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_DESCRIPTION";
            utilityRollDef.stockToConsume = 1;
            FixScriptableObjectName(utilityRollDef);
            SniperContent.entityStates.Add(typeof(CombatRoll));
            SniperContent.skillDefs.Add(utilityRollDef);
            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = utilityRollDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityRollDef.skillNameToken, false)
            };
            SniperContent.Skills.Utility.Roll = utilityRollDef;

            //Disabled this skill because of being jank and uninteractive. Feel free to give a shot at re-adding it if you think you can make it work.
            SkillDef utilitySmokeDef = SkillDef.CreateInstance<SkillDef>();
            utilitySmokeDef.activationState = new SerializableEntityStateType(typeof(AimSmokeGrenade));
            utilitySmokeDef.activationStateMachineName = "Scope";
            utilitySmokeDef.baseMaxStock = 1;
            utilitySmokeDef.baseRechargeInterval = 12f;
            utilitySmokeDef.beginSkillCooldownOnSkillEnd = true;
            utilitySmokeDef.canceledFromSprinting = false;
            utilitySmokeDef.cancelSprintingOnActivation = false;
            utilitySmokeDef.dontAllowPastMaxStocks = true;
            utilitySmokeDef.forceSprintDuringState = false;
            utilitySmokeDef.fullRestockOnAssign = true;
            utilitySmokeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilitySmoke.png");
            utilitySmokeDef.interruptPriority = InterruptPriority.PrioritySkill;
            utilitySmokeDef.isCombatSkill = false;
            utilitySmokeDef.keywordTokens = new string[] { };
            utilitySmokeDef.mustKeyPress = false;
            utilitySmokeDef.rechargeStock = 1;
            utilitySmokeDef.requiredStock = 1;
            utilitySmokeDef.skillName = "SmokeGrenade";
            utilitySmokeDef.skillNameToken = "SNIPERCLASSIC_UTILITY_SMOKE_NAME";
            utilitySmokeDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_SMOKE_DESCRIPTION";
            utilitySmokeDef.stockToConsume = 1;
            FixScriptableObjectName(utilitySmokeDef);
            /*Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = utilitySmokeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilitySmokeDef.skillNameToken, false)
            };*/
            SniperContent.skillDefs.Add(utilitySmokeDef);
            SniperContent.entityStates.Add(typeof(AimSmokeGrenade));
            SniperContent.entityStates.Add(typeof(FireSmokeGrenade));

            SniperContent.skillFamilies.Add(utilitySkillFamily);
        }
        private static void AssignSpecial(SkillLocator sk)
        {
            DroneStateMachineSetup();
            SpotterFollowerSetup();

            GameObject spotterIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PassiveHealing/WoodSpriteIndicator.prefab").WaitForCompletion().InstantiateClone("SniperClassicSpotterTargetingIndicator", false);
            SpriteRenderer sr = spotterIndicator.GetComponentInChildren<SpriteRenderer>();
            sr.sprite = SniperContent.assetBundle.LoadAsset<Sprite>("texSpotterMarkedOverlayUI.png");
            sr.color = new Color(1f, 61f / 255f, 61f / 255f, 0.4f);
            sr.transform.localScale = 0.2f * Vector3.one;   //0.4f default

            UnityEngine.Object.Destroy(spotterIndicator.GetComponentInChildren<RoR2.InputBindingDisplayController>());
            UnityEngine.Object.Destroy(spotterIndicator.GetComponentInChildren<TMPro.TextMeshPro>());

            SpotterTargetingController.targetIndicator = spotterIndicator;


            SkillFamily specialSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (specialSkillFamily as ScriptableObject).name = "special";
            specialSkillFamily.defaultVariantIndex = 0u;
            specialSkillFamily.variants = new SkillFamily.Variant[1];
            sk.special._skillFamily = specialSkillFamily;

            SkillDef specialSpotDef = SkillDef.CreateInstance<SkillDef>();
            specialSpotDef.activationState = new SerializableEntityStateType(typeof(SendSpotter));
            specialSpotDef.activationStateMachineName = "DroneLauncher";
            specialSpotDef.baseMaxStock = 1;
            specialSpotDef.baseRechargeInterval = 0.5f;
            specialSpotDef.beginSkillCooldownOnSkillEnd = true;
            specialSpotDef.canceledFromSprinting = false;
            specialSpotDef.dontAllowPastMaxStocks = true;
            specialSpotDef.forceSprintDuringState = false;
            specialSpotDef.fullRestockOnAssign = true;
            specialSpotDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialIcon.png");
            specialSpotDef.interruptPriority = InterruptPriority.Any;
            specialSpotDef.isCombatSkill = false;
            specialSpotDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotDef.mustKeyPress = true;
            specialSpotDef.cancelSprintingOnActivation = false;
            specialSpotDef.rechargeStock = 1;
            specialSpotDef.requiredStock = 0;
            specialSpotDef.skillName = "Spot";
            specialSpotDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_NAME";
            specialSpotDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_DESCRIPTION";
            specialSpotDef.stockToConsume = 0;
            FixScriptableObjectName(specialSpotDef);
            SniperContent.skillDefs.Add(specialSpotDef);
            Nemesis.specialSpotDef = specialSpotDef;
            SniperContent.Skills.Special.Feedback = specialSpotDef;

            SkillDef specialSpotReturnDef = SkillDef.CreateInstance<SkillDef>();
            specialSpotReturnDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.ReturnSpotter));
            specialSpotReturnDef.activationStateMachineName = "DroneLauncher";
            specialSpotReturnDef.baseMaxStock = 1;
            specialSpotReturnDef.baseRechargeInterval = 0f;
            specialSpotReturnDef.beginSkillCooldownOnSkillEnd = true;
            specialSpotReturnDef.canceledFromSprinting = false;
            specialSpotReturnDef.dontAllowPastMaxStocks = true;
            specialSpotReturnDef.forceSprintDuringState = false;
            specialSpotReturnDef.fullRestockOnAssign = true;
            specialSpotReturnDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialCancelIcon.png");
            specialSpotReturnDef.interruptPriority = InterruptPriority.Any;
            specialSpotReturnDef.isCombatSkill = false;
            specialSpotReturnDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotReturnDef.mustKeyPress = true;
            specialSpotReturnDef.cancelSprintingOnActivation = false;
            specialSpotReturnDef.rechargeStock = 1;
            specialSpotReturnDef.requiredStock = 0;
            specialSpotReturnDef.skillName = "ReturnSpot";
            specialSpotReturnDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_NAME";
            specialSpotReturnDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_DESCRIPTION";
            specialSpotReturnDef.stockToConsume = 0;
            FixScriptableObjectName(specialSpotReturnDef);
            SniperContent.skillDefs.Add(specialSpotReturnDef);

            EntityStates.SniperClassicSkills.SendSpotter.specialSkillDef = specialSpotReturnDef;

            SniperContent.entityStates.Add(typeof(SendSpotter));
            SniperContent.entityStates.Add(typeof(ReturnSpotter));
            specialSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = specialSpotDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(specialSpotDef.skillNameToken, false)
            };

            SkillDef specialSpotScepterDef = SkillDef.CreateInstance<SkillDef>();
            specialSpotScepterDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SendSpotterScepter));
            specialSpotScepterDef.activationStateMachineName = "DroneLauncher";
            specialSpotScepterDef.baseMaxStock = 1;
            specialSpotScepterDef.baseRechargeInterval = 0.5f;
            specialSpotScepterDef.beginSkillCooldownOnSkillEnd = true;
            specialSpotScepterDef.canceledFromSprinting = false;
            specialSpotScepterDef.dontAllowPastMaxStocks = true;
            specialSpotScepterDef.forceSprintDuringState = false;
            specialSpotScepterDef.fullRestockOnAssign = true;
            specialSpotScepterDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialScepterIcon.png");
            specialSpotScepterDef.interruptPriority = InterruptPriority.Any;
            specialSpotScepterDef.isCombatSkill = false;
            specialSpotScepterDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotScepterDef.mustKeyPress = true;
            specialSpotScepterDef.cancelSprintingOnActivation = false;
            specialSpotScepterDef.rechargeStock = 1;
            specialSpotScepterDef.requiredStock = 0;
            specialSpotScepterDef.skillName = "Spot";
            specialSpotScepterDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_SCEPTER_NAME";
            specialSpotScepterDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_SCEPTER_DESCRIPTION";
            specialSpotScepterDef.stockToConsume = 0;
            FixScriptableObjectName(specialSpotScepterDef);
            SniperContent.skillDefs.Add(specialSpotScepterDef);
            SniperContent.Skills.Special.FeedbackScepter = specialSpotScepterDef;
            SniperContent.entityStates.Add(typeof(SendSpotterScepter));

            if (Modules.Config.cursed)
            {
                SkillDef specialSpotDisruptDef = SkillDef.CreateInstance<SkillDef>();
                specialSpotDisruptDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SendSpotterDisrupt));
                specialSpotDisruptDef.activationStateMachineName = "DroneLauncher";
                specialSpotDisruptDef.baseMaxStock = 1;
                specialSpotDisruptDef.baseRechargeInterval = 10f;
                specialSpotDisruptDef.beginSkillCooldownOnSkillEnd = true;
                specialSpotDisruptDef.canceledFromSprinting = false;
                specialSpotDisruptDef.dontAllowPastMaxStocks = true;
                specialSpotDisruptDef.forceSprintDuringState = false;
                specialSpotDisruptDef.fullRestockOnAssign = true;
                specialSpotDisruptDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialDisruptIcon.png");
                specialSpotDisruptDef.interruptPriority = InterruptPriority.Any;
                specialSpotDisruptDef.isCombatSkill = false;
                specialSpotDisruptDef.keywordTokens = new string[] { "KEYWORD_STUNNING", "KEYWORD_SNIPERCLASSIC_ANALYZED" };
                specialSpotDisruptDef.mustKeyPress = true;
                specialSpotDisruptDef.cancelSprintingOnActivation = false;
                specialSpotDisruptDef.rechargeStock = 1;
                specialSpotDisruptDef.requiredStock = 1;
                specialSpotDisruptDef.skillName = "Spot";
                specialSpotDisruptDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_ALT_NAME";
                specialSpotDisruptDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_ALT_DESCRIPTION";
                specialSpotDisruptDef.stockToConsume = 1;
                FixScriptableObjectName(specialSpotDisruptDef);
                Nemesis.specialSpotDisruptDef = specialSpotDisruptDef;
                SniperContent.skillDefs.Add(specialSpotDisruptDef);
                SniperContent.entityStates.Add(typeof(SendSpotterDisrupt));
                Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
                specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
                {
                    skillDef = specialSpotDisruptDef,
                    unlockableName = "",
                    viewableNode = new ViewablesCatalog.Node(specialSpotDisruptDef.skillNameToken, false)
                };
                SniperContent.Skills.Special.Disrupt = specialSpotDisruptDef;

                SkillDef specialSpotDisruptScepterDef = SkillDef.CreateInstance<SkillDef>();
                specialSpotDisruptScepterDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SendSpotterDisruptScepter));
                specialSpotDisruptScepterDef.activationStateMachineName = "DroneLauncher";
                specialSpotDisruptScepterDef.baseMaxStock = 1;
                specialSpotDisruptScepterDef.baseRechargeInterval = 10f;
                specialSpotDisruptScepterDef.beginSkillCooldownOnSkillEnd = true;
                specialSpotDisruptScepterDef.canceledFromSprinting = false;
                specialSpotDisruptScepterDef.dontAllowPastMaxStocks = true;
                specialSpotDisruptScepterDef.forceSprintDuringState = false;
                specialSpotDisruptScepterDef.fullRestockOnAssign = true;
                specialSpotDisruptScepterDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialDisruptScepterIcon.png");
                specialSpotDisruptScepterDef.interruptPriority = InterruptPriority.Any;
                specialSpotDisruptScepterDef.isCombatSkill = false;
                specialSpotDisruptScepterDef.keywordTokens = new string[] { "KEYWORD_SHOCKING", "KEYWORD_SNIPERCLASSIC_ANALYZED" };
                specialSpotDisruptScepterDef.mustKeyPress = true;
                specialSpotDisruptScepterDef.cancelSprintingOnActivation = false;
                specialSpotDisruptScepterDef.rechargeStock = 1;
                specialSpotDisruptScepterDef.requiredStock = 1;
                specialSpotDisruptScepterDef.skillName = "Spot";
                specialSpotDisruptScepterDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_NAME";
                specialSpotDisruptScepterDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_ALT_SCEPTER_DESCRIPTION";
                specialSpotDisruptScepterDef.stockToConsume = 1;
                FixScriptableObjectName(specialSpotDisruptScepterDef);
                SniperContent.entityStates.Add(typeof(SendSpotterDisruptScepter));
                SniperContent.Skills.Special.DisruptScepter = specialSpotDisruptScepterDef;
                SniperContent.skillDefs.Add(specialSpotDisruptScepterDef);
            }
        }
        private static void DroneStateMachineSetup()
        {
            EntityStateMachine droneMachine = SniperClassic.SniperBody.AddComponent<EntityStateMachine>();
            droneMachine.customName = "DroneLauncher";
            droneMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));
            droneMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));
            NetworkStateMachine nsm = SniperClassic.SniperBody.GetComponent<NetworkStateMachine>();
            nsm.stateMachines = nsm.stateMachines.Append(droneMachine).ToArray();
        }

        private static void SpotterFollowerSetup()
        {
            GameObject spotterObject = SniperContent.assetBundle.LoadAsset<GameObject>("mdlSpotter.prefab");
            spotterObject.AddComponent<NetworkIdentity>();
            spotterObject.AddComponent<SpotterFollowerController>();

            spotterObject.RegisterNetworkPrefab();

            Modules.SniperContent.networkedObjectPrefabs.Add(spotterObject);

            SpotterTargetingController.spotterFollowerGameObject = spotterObject;
            spotterObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = Modules.Assets.CreateMaterial("matSniperDefault", 3f, new Color(1f, 163f / 255f, 92f / 255f));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void SetupScepterSkills()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SniperContent.Skills.Special.FeedbackScepter, "SniperClassicBody", SkillSlot.Special, 0);
            if (Modules.Config.cursed)
                AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SniperContent.Skills.Special.DisruptScepter, "SniperClassicBody", SkillSlot.Special, 1);
            //Add cases for Nemesis Sniper when implemented.
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void SetupScepterClassicSkills()
        {
            ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(SniperContent.Skills.Special.FeedbackScepter, "SniperClassicBody", SkillSlot.Special, SniperContent.Skills.Special.Feedback);
            if (Modules.Config.cursed)
                ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(SniperContent.Skills.Special.DisruptScepter, "SniperClassicBody", SkillSlot.Special, SniperContent.Skills.Special.Disrupt);
            //Add cases for Nemesis Sniper when implemented.
        }

        private static void FixScriptableObjectName(SkillDef sk)
        {
            (sk as ScriptableObject).name = sk.skillName;
        }
    }
}
