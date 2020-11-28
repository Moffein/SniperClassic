using BepInEx;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.SniperClassicSkills;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Orbs;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SniperClassic
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.SniperClassic", "Sniper Classic", "0.0.2")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(PrefabAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    
    public class SniperClassic : BaseUnityPlugin
    {
        GameObject SniperBody = null;
        Sprite iconPrimary = null;
        Sprite iconSecondary = null;
        Sprite iconUtility = null;
        Sprite iconSpecial = null;
        Sprite iconSpecialReturn = null;
        Sprite iconReload = null;
        Color SniperColor = new Color(78f / 255f, 80f / 255f, 111f / 255f);
        const String assetPrefix = "@MoffeinSniperClassic";
        const String portraitPath = assetPrefix + ":sniper2.png";
        const String textureBarPath = assetPrefix + ":reloadbar.png";
        const String textureCursorPath = assetPrefix + ":reloadslider.png";
        const String textureReloadGoodPath = assetPrefix + ":reload_good.png";
        const String textureReloadPerfectPath = assetPrefix + ":reload_perfect.png";
        const String textureIconSpecialReturnPath = assetPrefix + ":skill4_return_hd.png";
        const String textureIconReloadPath = assetPrefix + ":skill1_reload_hd.png";
        const String textureIconPrimaryPath = assetPrefix + ":skill1_version2.png";
        const String textureIconSecondaryPath = assetPrefix + ":skill2.png";
        const String textureIconUtilityPath = assetPrefix + ":skill3.png";
        const String textureIconSpecialPath = assetPrefix + ":skill4.png";
        Texture2D sniperIcon = null;

        public static BuffIndex spotterStatDebuff;
        public static BuffIndex spotterBuff;
        public static BuffIndex spotterCooldownBuff;

        public void Awake()
        {
            Setup();

            On.RoR2.UI.SniperScopeChargeIndicatorController.FixedUpdate += (orig, self) =>
            {
                orig(self);
                if (self.image)
                {
                    ScopeController sc = self.hudElement.targetBodyObject.GetComponent<ScopeController>();
                    if (sc)
                    {
                        self.image.fillAmount = sc.charge;
                    }
                }
            };

            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                if (self.HasBuff(spotterStatDebuff))
                {
                    self.SetPropertyValue<float>("armor", self.armor - 20f);
                    self.SetPropertyValue<float>("moveSpeed", self.moveSpeed * 0.6f);
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                CharacterBody victimBody = null;
                bool hadSpotter = false;
                if (victim)
                {
                    victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(spotterBuff) || victimBody.HasBuff(spotterCooldownBuff))
                        {
                            if (victimBody.HasBuff(spotterBuff))
                            {
                                hadSpotter = true;
                            }
                        }
                    }
                 }
                orig(self, damageInfo, victim);
                if (!damageInfo.rejected && hadSpotter)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            if (damageInfo.procCoefficient > 0f && damageInfo.damage / attackerBody.damage >= 4f)
                            {
                                LightningOrb spotterLightning = new LightningOrb
                                {
                                    attacker = damageInfo.attacker,
                                    inflictor = damageInfo.attacker,
                                    damageValue = damageInfo.damage * 0.4f,
                                    procCoefficient = 0.33f,
                                    teamIndex = attackerBody.teamComponent.teamIndex,
                                    isCrit = damageInfo.crit,
                                    procChainMask = damageInfo.procChainMask,
                                    lightningType = LightningOrb.LightningType.Tesla,
                                    damageColorIndex = DamageColorIndex.WeakPoint,
                                    bouncesRemaining = 5,
                                    targetsToFindPerBounce = 5,
                                    range = 20f,
                                    origin = damageInfo.position,
                                    damageType = (DamageType.SlowOnHit | DamageType.Stun1s),
                                    speed = 120f
                                };

                                spotterLightning.bouncedObjects = new List<HealthComponent>();

                                if (victimBody && victimBody.healthComponent && victimBody.healthComponent.alive)
                                {
                                    victimBody.RemoveBuff(spotterBuff);
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        victimBody.AddTimedBuff(spotterCooldownBuff, i);
                                    }
                                    //spotterLightning.bouncedObjects.Add(victimBody.healthComponent);
                                }
                                HurtBox hurtBox = spotterLightning.PickNextTarget(damageInfo.position);
                                if (hurtBox)
                                {
                                    spotterLightning.target = hurtBox;
                                    OrbManager.instance.AddOrb(spotterLightning);
                                }
                            }
                        }
                    }
                }
            };
        }

        public void Setup()
        {
            LoadResources();
            RegisterLanguageTokens();
            SetupBody();
            SetupStats();
            AddSkin();
            AssignSkills();
            RegisterSurvivor();
            CreateBuffs();
        }

        public void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SNIPERCLASSIC_BODY_NAME", "Sniper");
            LanguageAPI.Add("SNIPERCLASSIC_BODY_SUBTITLE", "Eagle Eye");
            LanguageAPI.Add("SNIPERCLASSIC_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_NAME", "Snipe");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_DESCRIPTION", "<style=cIsUtility>Agile</style>. Fire a piercing shot for <style=cIsDamage>350% damage</style>. After firing, <style=cIsDamage>reload your weapon</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly.");

            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_NAME", "Steady Aim");
            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Carefully take aim, <style=cIsDamage>increasing the damage</style> of your next shot up to <style=cIsDamage>4.0x</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_NAME", "Military Training");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_DESCRIPTION", "<style=cIsUtility>Roll</style> a short distance and <style=cIsDamage>instantly reload your weapon</style>.");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_NAME", "Spotter: FEEDBACK");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style> with your Spotter, reducing their movement speed and armor. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to transfer <style=cIsDamage>40% TOTAL damage</style> to all enemies near your Spotter (Recharges every <style=cIsUtility>10</style> seconds).");

            LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_ANALYZED", "<style=cKeywordName>Analyzed</style><style=cSub>Reduce movement speed by <style=cIsDamage>40%</style> and reduce armor by <style=cIsDamage>20</style>. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to transfer <style=cIsDamage>40% TOTAL damage</style> to all enemies near your Spotter (Recharges every <style=cIsUtility>10</style> seconds).</style>");

            LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR", "..and so they left, the sound still ringing in deaf ears.");
            LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR_JOKE", "..and so they left, having never picked up a weel gun.");

            String sniperDesc = "";
            sniperDesc += "The Sniper is an marksman who works with his trusty Spotter drone to eliminate targets from afar.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Purchase Drones to cover your back you while you snipe." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Snipe must be reloaded after every shot. Learn the timing to maximize your damage output!" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Military Training allows you to escape from danger while charging Steady Aim." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Steady Aim combined with Spotter: FEEDBACK and a perfectly reloaded Snipe can deal massive damage to crowds of enemies." + Environment.NewLine + Environment.NewLine;
            LanguageAPI.Add("SNIPERCLASSIC_DESCRIPTION", sniperDesc);

            //String tldr = "bio: sniper was born with a special power he was stronger than all his crewmates at the ues contact light. he served in the risk of rain military fighting providence and in the final battel against providence they were fighting and providence turned him to the darkness and sniper turned against HAN-D and kill him preventing him from being in the sequel. he killed his own spotter drone in the battle which is why it isn't here please stop PMing asking me why that's why. also capes are cool fuck you space_cowboy226 everyone knows youre a red item stealing faggot\n\nlikes: snipin, bein badass (gearbox style), crowbars, the reaper (from deadbolt), killing, death, dubstep, backflips, razor chroma key, hot huntresses with big boobys who dress like sluts, decoys, the reaper (from real life), railguns, capes (the cool kind not the gay kind)\n\ndislikes: spotter drones, wisps, frenzied elites, the enforcer from ues fuck you enforcer stop showin everyone my deviantart logs you peace of shit, mithrix, desk plants, space_cowboy226 (mega ass-faggot), rain, life, the captain, overloading worms";
            //LanguageAPI.Add("SNIPERCLASSIC_LORE", tldr);
        }

        public void RegisterSurvivor()
        {
            GameObject SniperDisplay = SniperBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            SurvivorDef item = new SurvivorDef
            {
                name = "SniperClassic",
                bodyPrefab = SniperBody,
                descriptionToken = "SNIPERCLASSIC_DESCRPTION",
                displayPrefab = SniperDisplay,
                primaryColor = SniperColor,
                unlockableName = "",
                outroFlavorToken = "SNIPERCLASSIC_OUTRO_FLAVOR"
            };
            SurvivorAPI.AddSurvivor(item);
        }

        public void SetupBody()
        {
            if (!SniperBody)
            {
                SniperBody = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody").InstantiateClone("SniperClassic", true);
                BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
                {
                    list.Add(SniperBody);
                };
            }
        }

        public void SetupStats()
        {
            if (SniperBody)
            {
                SniperBody.AddComponent<ScopeController>();
                SniperBody.AddComponent<ReloadController>();
                SniperBody.AddComponent<SpotterTargetingController>();
                CharacterBody cb = SniperBody.GetComponent<CharacterBody>();
                if (cb)
                {
                    cb.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
                    cb.baseNameToken = "SNIPERCLASSIC_BODY_NAME";
                    cb.subtitleNameToken = "SNIPERCLASSIC_BODY_SUBTITLE";
                    cb.crosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/StandardCrosshair");
                    cb.baseMaxHealth = 110f;
                    cb.baseRegen = 1f;
                    cb.baseMaxShield = 0f;
                    cb.baseMoveSpeed = 7f;
                    cb.baseAcceleration = 80f;
                    cb.baseJumpPower = 15f;
                    cb.baseDamage = 15f;
                    cb.baseAttackSpeed = 1f;
                    cb.baseCrit = 1f;
                    cb.baseArmor = 0f;
                    cb.baseJumpCount = 1;
                    cb.tag = "Player";

                    cb.autoCalculateLevelStats = true;
                    cb.levelMaxHealth = cb.baseMaxHealth * 0.3f;
                    cb.levelRegen = cb.levelRegen * 0.2f;
                    cb.levelMaxShield = 0f;
                    cb.levelMoveSpeed = 0f;
                    cb.levelJumpPower = 0f;
                    cb.levelDamage = cb.baseDamage * 0.2f;
                    cb.levelAttackSpeed = 0f;
                    cb.levelCrit = 0f;
                    cb.levelArmor = 0f;

                    cb.portraitIcon = sniperIcon;
                }
            }
        }

        public void AddSkin()    //credits to rob
        {
            GameObject bodyPrefab = SniperBody;
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = null;
            if (model.GetComponent<ModelSkinController>())
                skinController = model.GetComponent<ModelSkinController>();
            else
                skinController = model.AddComponent<ModelSkinController>();

            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");
            if (mainRenderer == null)
            {
                CharacterModel.RendererInfo[] bRI = Reflection.GetFieldValue<CharacterModel.RendererInfo[]>(characterModel, "baseRendererInfos");
                if (bRI != null)
                {
                    foreach (CharacterModel.RendererInfo rendererInfo in bRI)
                    {
                        if (rendererInfo.renderer is SkinnedMeshRenderer)
                        {
                            mainRenderer = (SkinnedMeshRenderer)rendererInfo.renderer;
                            break;
                        }
                    }
                    if (mainRenderer != null)
                    {
                        characterModel.SetFieldValue<SkinnedMeshRenderer>("mainSkinnedMeshRenderer", mainRenderer);
                    }
                }
            }

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f/255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), SniperColor);
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            skinDefInfo.Name = "SNIPERCLASSIC_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "SNIPERCLASSIC_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableName = "";
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[1]
            {
                defaultSkin,
            };
        }

        public void AssignSkills()
        {
            if (SniperBody)
            {
                SkillLocator sk = SniperBody.GetComponent<SkillLocator>();
                if (sk)
                {
                    sk.passiveSkill.enabled = false;
                    AssignPrimary(sk);
                    AssignSecondary(sk);
                    AssignUtility(sk);
                    AssignSpecial(sk);
                }
            }
        }

        public void AssignPrimary(SkillLocator sk)
        {
            SkillFamily primarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            primarySkillFamily.defaultVariantIndex = 0u;
            primarySkillFamily.variants = new SkillFamily.Variant[1];
            Reflection.SetFieldValue<SkillFamily>(sk.primary, "_skillFamily", primarySkillFamily);

            SkillDef primarySnipeDef = SkillDef.CreateInstance<SkillDef>();
            primarySnipeDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.Snipe));
            primarySnipeDef.activationStateMachineName = "Weapon";
            primarySnipeDef.baseMaxStock = 1;
            primarySnipeDef.baseRechargeInterval = 0f;
            primarySnipeDef.beginSkillCooldownOnSkillEnd = false;
            primarySnipeDef.canceledFromSprinting = false;
            primarySnipeDef.dontAllowPastMaxStocks = true;
            primarySnipeDef.forceSprintDuringState = false;
            primarySnipeDef.fullRestockOnAssign = true;
            primarySnipeDef.icon = iconPrimary;
            primarySnipeDef.interruptPriority = InterruptPriority.Any;
            primarySnipeDef.isBullets = true;
            primarySnipeDef.isCombatSkill = true;
            primarySnipeDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            primarySnipeDef.mustKeyPress = true;
            primarySnipeDef.noSprint = false;
            primarySnipeDef.rechargeStock = 1;
            primarySnipeDef.requiredStock = 1;
            primarySnipeDef.shootDelay = 0f;
            primarySnipeDef.skillName = "Snipe";
            primarySnipeDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_NAME";
            primarySnipeDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_DESCRIPTION";
            primarySnipeDef.stockToConsume = 1;

            ReloadSnipe.reloadIcon = iconReload;

            LoadoutAPI.AddSkill(typeof(EntityStates.SniperClassicSkills.Snipe));
            LoadoutAPI.AddSkill(typeof(EntityStates.SniperClassicSkills.ReloadSnipe));

            LoadoutAPI.AddSkillDef(primarySnipeDef);
            LoadoutAPI.AddSkillFamily(primarySkillFamily);
            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primarySnipeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySnipeDef.skillNameToken, false)
            };
        }

        public void AssignSecondary(SkillLocator sk)
        {
            ScopeCrosshairSetup();
            ScopeStateMachineSetup();

            SkillFamily secondarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            secondarySkillFamily.defaultVariantIndex = 0u;
            secondarySkillFamily.variants = new SkillFamily.Variant[1];
            Reflection.SetFieldValue<SkillFamily>(sk.secondary, "_skillFamily", secondarySkillFamily);

            SkillDef secondaryScopeDef = SkillDef.CreateInstance<SkillDef>();
            secondaryScopeDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SecondaryScope));
            secondaryScopeDef.activationStateMachineName = "Scope";
            secondaryScopeDef.baseMaxStock = 1;
            secondaryScopeDef.baseRechargeInterval = 4f;
            secondaryScopeDef.beginSkillCooldownOnSkillEnd = false;
            secondaryScopeDef.canceledFromSprinting = false;
            secondaryScopeDef.dontAllowPastMaxStocks = true;
            secondaryScopeDef.forceSprintDuringState = false;
            secondaryScopeDef.fullRestockOnAssign = true;
            secondaryScopeDef.icon = iconSecondary;
            secondaryScopeDef.interruptPriority = InterruptPriority.Any;
            secondaryScopeDef.isBullets = false;
            secondaryScopeDef.isCombatSkill = false;
            secondaryScopeDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            secondaryScopeDef.mustKeyPress = false;
            secondaryScopeDef.noSprint = true;
            secondaryScopeDef.rechargeStock = 1;
            secondaryScopeDef.requiredStock = 0;
            secondaryScopeDef.shootDelay = 0f;
            secondaryScopeDef.skillName = "Scope";
            secondaryScopeDef.skillNameToken = "SNIPERCLASSIC_SECONDARY_NAME";
            secondaryScopeDef.skillDescriptionToken = "SNIPERCLASSIC_SECONDARY_DESCRIPTION";
            secondaryScopeDef.stockToConsume = 0;

            LoadoutAPI.AddSkill(typeof(SecondaryScope));

            LoadoutAPI.AddSkillDef(secondaryScopeDef);
            LoadoutAPI.AddSkillFamily(secondarySkillFamily);
            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondaryScopeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondaryScopeDef.skillNameToken, false)
            };
        }
        public void ScopeCrosshairSetup()
        {
            EntityStates.SniperClassicSkills.SecondaryScope.crosshairPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/crosshair/snipercrosshair"), "SniperClassicScopeCrosshair", false);
            DisplayStock ds = EntityStates.SniperClassicSkills.SecondaryScope.crosshairPrefab.GetComponent<DisplayStock>();
            ds.skillSlot = SkillSlot.Secondary;
        }
        public void ScopeStateMachineSetup()
        {
            EntityStateMachine scopeMachine = SniperBody.AddComponent<EntityStateMachine>();
            scopeMachine.customName = "Scope";
            scopeMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            scopeMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
        }

        public void AssignUtility(SkillLocator sk)
        {
            SkillFamily utilitySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            utilitySkillFamily.defaultVariantIndex = 0u;
            utilitySkillFamily.variants = new SkillFamily.Variant[1];
            Reflection.SetFieldValue<SkillFamily>(sk.utility, "_skillFamily", utilitySkillFamily);

            SkillDef utilityRollDef = SkillDef.CreateInstance<SkillDef>();
            utilityRollDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.CombatRoll));
            utilityRollDef.activationStateMachineName = "Body";
            utilityRollDef.baseMaxStock = 1;
            utilityRollDef.baseRechargeInterval = 6f;
            utilityRollDef.beginSkillCooldownOnSkillEnd = false;
            utilityRollDef.canceledFromSprinting = false;
            utilityRollDef.dontAllowPastMaxStocks = true;
            utilityRollDef.forceSprintDuringState = false;
            utilityRollDef.fullRestockOnAssign = true;
            utilityRollDef.icon = iconUtility;
            utilityRollDef.interruptPriority = InterruptPriority.Any;
            utilityRollDef.isBullets = false;
            utilityRollDef.isCombatSkill = false;
            utilityRollDef.keywordTokens = new string[] { };
            utilityRollDef.mustKeyPress = false;
            utilityRollDef.noSprint = false;
            utilityRollDef.rechargeStock = 1;
            utilityRollDef.requiredStock = 1;
            utilityRollDef.shootDelay = 0f;
            utilityRollDef.skillName = "CombatRoll";
            utilityRollDef.skillNameToken = "SNIPERCLASSIC_UTILITY_NAME";
            utilityRollDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_DESCRIPTION";
            utilityRollDef.stockToConsume = 1;
            LoadoutAPI.AddSkill(typeof(CombatRoll));
            LoadoutAPI.AddSkillDef(utilityRollDef);
            LoadoutAPI.AddSkillFamily(utilitySkillFamily);
            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = utilityRollDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityRollDef.skillNameToken, false)
            };
        }

        public void AssignSpecial(SkillLocator sk)
        {
            DroneStateMachineSetup();
            SpotterFollowerSetup();

            SkillFamily specialSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            specialSkillFamily.defaultVariantIndex = 0u;
            specialSkillFamily.variants = new SkillFamily.Variant[1];
            Reflection.SetFieldValue<SkillFamily>(sk.special, "_skillFamily", specialSkillFamily);

            SkillDef specialSpotDef = SkillDef.CreateInstance<SkillDef>();
            specialSpotDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SendSpotter));
            specialSpotDef.activationStateMachineName = "DroneLauncher";
            specialSpotDef.baseMaxStock = 1;
            specialSpotDef.baseRechargeInterval = 10f;
            specialSpotDef.beginSkillCooldownOnSkillEnd = true;
            specialSpotDef.canceledFromSprinting = false;
            specialSpotDef.dontAllowPastMaxStocks = true;
            specialSpotDef.forceSprintDuringState = false;
            specialSpotDef.fullRestockOnAssign = true;
            specialSpotDef.icon = iconSpecial;
            specialSpotDef.interruptPriority = InterruptPriority.Any;
            specialSpotDef.isBullets = false;
            specialSpotDef.isCombatSkill = false;
            specialSpotDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotDef.mustKeyPress = true;
            specialSpotDef.noSprint = false;
            specialSpotDef.rechargeStock = 1;
            specialSpotDef.requiredStock = 1;
            specialSpotDef.shootDelay = 0f;
            specialSpotDef.skillName = "Spot";
            specialSpotDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_NAME";
            specialSpotDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_DESCRIPTION";
            specialSpotDef.stockToConsume = 1;

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
            specialSpotReturnDef.icon = iconSpecialReturn;
            specialSpotReturnDef.interruptPriority = InterruptPriority.Any;
            specialSpotReturnDef.isBullets = false;
            specialSpotReturnDef.isCombatSkill = false;
            specialSpotReturnDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotReturnDef.mustKeyPress = true;
            specialSpotReturnDef.noSprint = false;
            specialSpotReturnDef.rechargeStock = 1;
            specialSpotReturnDef.requiredStock = 1;
            specialSpotReturnDef.shootDelay = 0f;
            specialSpotReturnDef.skillName = "ReturnSpot";
            specialSpotReturnDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_NAME";
            specialSpotReturnDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_DESCRIPTION";
            specialSpotReturnDef.stockToConsume = 1;

            EntityStates.SniperClassicSkills.SendSpotter.specialSkillDef = specialSpotReturnDef;

            LoadoutAPI.AddSkill(typeof(SendSpotter));
            LoadoutAPI.AddSkill(typeof(ReturnSpotter));
            specialSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = specialSpotDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(specialSpotDef.skillNameToken, false)
            };
        }
        public void DroneStateMachineSetup()
        {
            EntityStateMachine droneMachine = SniperBody.AddComponent<EntityStateMachine>();
            droneMachine.customName = "DroneLauncher";
            droneMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            droneMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
        }

        public void SpotterFollowerSetup()
        {
            SpotterTargetingController.spotterFollowerGameObject = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/HealingFollower"), "SniperClassicSpotter", true);
            Destroy(SpotterTargetingController.spotterFollowerGameObject.GetComponent<HealingFollowerController>());
            SpotterFollowerController sfc = SpotterTargetingController.spotterFollowerGameObject.AddComponent<SpotterFollowerController>();
        }

        public void CreateBuffs()
        {
            BuffDef spotterDef = new BuffDef
            {
                buffColor = new Color(0.8392157f, 0.227450982f, 0.227450982f),
                buffIndex = BuffIndex.Count,
                canStack = false,
                eliteIndex = EliteIndex.None,
                iconPath = "Textures/BuffIcons/texBuffCloakIcon",
                isDebuff = false,
                name = "SniperClassicSpotted"
            };
            SniperClassic.spotterBuff = BuffAPI.Add(new CustomBuff(spotterDef));

            BuffDef spotterCooldownDef = new BuffDef
            {
                buffColor = new Color(0.4f, 0.4f, 0.4f),
                buffIndex = BuffIndex.Count,
                canStack = true,
                eliteIndex = EliteIndex.None,
                iconPath = "Textures/BuffIcons/texBuffCloakIcon",
                isDebuff = false,
                name = "SniperClassicSpottedCooldown"
            };
            SniperClassic.spotterCooldownBuff = BuffAPI.Add(new CustomBuff(spotterCooldownDef));

            BuffDef spotterStatDebuffDef = new BuffDef
            {
                buffColor = new Color(0.8392157f, 0.227450982f, 0.227450982f),
                buffIndex = BuffIndex.Count,
                canStack = false,
                eliteIndex = EliteIndex.None,
                iconPath = "Textures/BuffIcons/texBuffWeakIcon",
                isDebuff = true,
                name = "SniperClassicSpottedStatDebuff"
            };
            SniperClassic.spotterStatDebuff = BuffAPI.Add(new CustomBuff(spotterStatDebuffDef));
        }

        public void LoadResources()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SniperClassic.sniperprofile"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new R2API.AssetBundleResourcesProvider(assetPrefix, bundle);
                R2API.ResourcesAPI.AddProvider(provider);
            }
            sniperIcon = Resources.Load<Texture2D>(portraitPath);
            ReloadController.reloadBar = Resources.Load<Texture2D>(textureBarPath);
            ReloadController.reloadCursor = Resources.Load<Texture2D>(textureCursorPath);
            ReloadController.indicatorGood = Resources.Load<Texture2D>(textureReloadGoodPath);
            ReloadController.indicatorPerfect = Resources.Load<Texture2D>(textureReloadPerfectPath);
            iconSpecialReturn = Resources.Load<Sprite>(textureIconSpecialReturnPath);
            iconReload = Resources.Load<Sprite>(textureIconReloadPath);
            iconPrimary = Resources.Load<Sprite>(textureIconPrimaryPath);
            iconSecondary = Resources.Load<Sprite>(textureIconSecondaryPath);
            iconUtility = Resources.Load<Sprite>(textureIconUtilityPath);
            iconSpecial = Resources.Load<Sprite>(textureIconSpecialPath);
        }
    }
}
