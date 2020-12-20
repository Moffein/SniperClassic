using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Bison;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.SniperClassicSkills;
using R2API;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SniperClassic
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.SniperClassic", "Sniper Classic", "0.3.2")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(PrefabAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    
    public class SniperClassic : BaseUnityPlugin
    {
        GameObject SniperBody = null;
        Sprite iconPrimary = null;
        Sprite iconSecondary = null;
        Sprite iconUtility = null;
        Sprite iconUtilitySmoke = null;
        Sprite iconSpecial = null;
        Sprite iconSpecialReturn = null;
        Sprite iconReload = null;
        Sprite iconPrimaryAlt = null;
        Image chargeImage = null;
        Color SniperColor = new Color(78f / 255f, 80f / 255f, 111f / 255f);
        const string assetPrefix = "@MoffeinSniperClassic";
        const string portraitPath = assetPrefix + ":sniper2.png";
        const string textureBarPath = assetPrefix + ":reloadbar.png";
        const string textureCursorPath = assetPrefix + ":reloadslider.png";
        const string textureBarFailPath = assetPrefix + ":reloadbar_failed.png";
        const string textureCursorFailPath = assetPrefix + ":reloadslider_failed.png";
        const string textureReloadEmptyPath = assetPrefix + ":reload_empty.png";
        const string textureReloadGoodPath = assetPrefix + ":reload_good_hd.png";
        const string textureReloadPerfectPath = assetPrefix + ":reload_perfect_hd.png";
        const string textureIconSpecialReturnPath = assetPrefix + ":skill4_return_hd.png";
        const string textureIconReloadPath = assetPrefix + ":skill1_reload_hd.png";
        const string textureIconPrimaryPath = assetPrefix + ":skill1.png";
        const string textureIconPrimaryAltPath = assetPrefix + ":skill1_version2.png";
        const string textureIconSecondaryPath = assetPrefix + ":skill2.png";
        const string textureIconUtilityPath = assetPrefix + ":skill3.png";
        //const string textureIconUtilitySmokePath = assetPrefix + ":banditsmoke.png";
        const string textureIconSpecialPath = assetPrefix + ":skill4.png";
        const string mdlSpotterPath = assetPrefix + ":mdlSpotter.prefab";
        const string noscopeCrosshairPath = assetPrefix + ":NoscopeCrosshair.prefab";
        const string scopeCrosshairPath = assetPrefix + ":ScopeCrosshair.prefab";
        //const string smokeGrenadePath = assetPrefix + ":TearGasGrenade.prefab";
        //const string smokeEffectPath = assetPrefix + ":TearGasEffect.prefab";

        GameObject noscopeCrosshair = null;
        GameObject scopeCrosshair = null;

        Texture2D sniperIcon = null;

        public static BuffIndex spotterStatDebuff;
        public static BuffIndex spotterBuff;
        public static BuffIndex spotterCooldownBuff;

        public void Awake()
        {
            Setup();

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
                                    range = 20f * damageInfo.procCoefficient,
                                    origin = damageInfo.position,
                                    damageType = (DamageType.SlowOnHit | DamageType.Stun1s),
                                    speed = 120f
                                };

                                spotterLightning.bouncedObjects = new List<HealthComponent>();

                                if (victimBody && victimBody.healthComponent)
                                {
                                    if (victimBody.healthComponent.alive)
                                    {
                                        victimBody.RemoveBuff(spotterBuff);
                                        for (int i = 1; i <= 10; i++)
                                        {
                                            victimBody.AddTimedBuff(spotterCooldownBuff, i);
                                        }
                                    }
                                    else
                                    {
                                        spotterLightning.bouncedObjects.Add(victimBody.healthComponent);
                                    }
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
            CreateBuffs();
            ReadConfig();
            SetupBody();
            SetupStats();
            //BuildGrenadePrefab();
            AddSkin();
            AssignSkills();
            RegisterSurvivor();
            RegisterLanguageTokens();
        }

        public void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SNIPERCLASSIC_BODY_NAME", "Sniper");
            LanguageAPI.Add("SNIPERCLASSIC_BODY_SUBTITLE", "Eagle Eye");
            LanguageAPI.Add("SNIPERCLASSIC_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_NAME", "Snipe");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_DESCRIPTION", "<style=cIsUtility>Agile</style>. Fire a piercing shot for <style=cIsDamage>360% damage</style>. After firing, <style=cIsDamage>reload your weapon</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly.");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_NAME", "Mark");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>320% damage</style>. After emptying your clip, <style=cIsDamage>reload your weapon</style> and <style=cIsUtility>gain 1 charge of Steady Aim</style> if perfectly timed.");


            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_NAME", "Steady Aim");

            string secondaryDesc = "Carefully take aim, <style=cIsDamage>increasing the damage</style> of your next shot up to <style=cIsDamage>4.0x</style>. Fully charged shots <style=cIsDamage>stun</style>.";
            if (SecondaryScope.useScrollWheelZoom)
            {
                secondaryDesc += " Use the scroll wheel to change zoom level.";
            }
            LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_DESCRIPTION", secondaryDesc);

            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_NAME", "Military Training");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_DESCRIPTION", "<style=cIsUtility>Roll</style> a short distance and <style=cIsDamage>instantly reload your weapon</style>.");

            /*LanguageAPI.Add("SNIPERCLASSIC_UTILITY_ALT_NAME", "Smokescreen");
            LanguageAPI.Add("SNIPERCLASSIC_UTILITY_ALT_DESCRIPTION", "Throw a smoke grenade that <style=cIsDamage>slows enemies</style> and conceals allies, making them <style=cIsUtility>invisible</style>.");*/

            //LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_INVIS", "<style=cKeywordName>Invisible</style><style=cSub>Enemies are unable to target you.</style>");

            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_NAME", "Spotter: FEEDBACK");
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style> with your Spotter, reducing their movement speed and armor. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to transfer <style=cIsDamage>40% TOTAL damage</style> to all enemies near your Spotter (Recharges every <style=cIsUtility>10</style> seconds).");

            LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_ANALYZED", "<style=cKeywordName>Analyzed</style><style=cSub>Reduce movement speed by <style=cIsDamage>40%</style> and reduce armor by <style=cIsDamage>20</style>. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to transfer <style=cIsDamage>40% TOTAL damage</style> to all enemies near your Spotter (Recharges every <style=cIsUtility>10</style> seconds).</style>");

            LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR", "..and so they left, the sound still ringing in deaf ears.");
            LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR_JOKE", "..and so they left, having never picked up a weel gun.");

            String sniperDesc = "";
            sniperDesc += "The Sniper is an marksman who works with his trusty Spotter drone to eliminate targets from afar.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Snipe must be reloaded after every shot. Learn the timing to maximize your damage output!" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Military Training allows you to escape from danger while charging Steady Aim." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Steady Aim combined with Spotter: FEEDBACK and a perfectly reloaded Snipe can wipe out crowds of enemies." + Environment.NewLine + Environment.NewLine;
            LanguageAPI.Add("SNIPERCLASSIC_DESCRIPTION", sniperDesc);

            String tldr = "bio: sniper was born with a special power he was stronger than all his crewmates at the ues contact light. he served in the risk of rain military fighting providence and in the final battel against providence they were fighting and providence turned him to the darkness and sniper turned against HAN-D and kill him preventing him from being in the sequel. he killed his own spotter drone in the battle which is why it isn't here please stop PMing asking me why that's why. also capes are cool fuck you space_cowboy226 everyone knows youre a red item stealing faggot\n\nlikes: snipin, bein badass (gearbox style), crowbars, the reaper (from deadbolt), killing, death, dubstep, backflips, razor chroma key, hot huntresses with big boobys who dress like sluts, decoys, the reaper (from real life), railguns, capes (the cool kind not the gay kind)\n\ndislikes: spotter drones, wisps, frenzied elites, the enforcer from ues fuck you enforcer stop showin everyone my deviantart logs you peace of shit, mithrix, desk plants, space_cowboy226 (mega ass-faggot), rain, life, the captain, overloading worms";
            LanguageAPI.Add("SNIPERCLASSIC_BODY_LORE", tldr);
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
                SniperBody = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody").InstantiateClone("SniperClassicBody", true);
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
                    cb.levelRegen = cb.baseRegen * 0.2f;
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
            primarySnipeDef.icon = iconPrimaryAlt;
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
            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primarySnipeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySnipeDef.skillNameToken, false)
            };

            SkillDef primaryBRDef = SkillDef.CreateInstance<SkillDef>();
            primaryBRDef.activationState = new SerializableEntityStateType(typeof(FireBattleRifle));
            primaryBRDef.activationStateMachineName = "Weapon";
            primaryBRDef.baseMaxStock = 6;
            primaryBRDef.baseRechargeInterval = 0f;
            primaryBRDef.beginSkillCooldownOnSkillEnd = false;
            primaryBRDef.canceledFromSprinting = false;
            primaryBRDef.dontAllowPastMaxStocks = true;
            primaryBRDef.forceSprintDuringState = false;
            primaryBRDef.fullRestockOnAssign = true;
            primaryBRDef.icon = iconPrimary;
            primaryBRDef.interruptPriority = InterruptPriority.Any;
            primaryBRDef.isBullets = false;
            primaryBRDef.isCombatSkill = true;
            primaryBRDef.keywordTokens = new string[] {};
            primaryBRDef.mustKeyPress = false;
            primaryBRDef.noSprint = true;
            primaryBRDef.rechargeStock = 0;
            primaryBRDef.requiredStock = 1;
            primaryBRDef.shootDelay = 0f;
            primaryBRDef.skillName = "BattleRifle";
            primaryBRDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT_NAME";
            primaryBRDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION";
            primaryBRDef.stockToConsume = 1;
            ReloadBR.reloadIcon = iconReload;
            LoadoutAPI.AddSkillDef(primaryBRDef);
            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primaryBRDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryBRDef.skillNameToken, false)
            };
            LoadoutAPI.AddSkill(typeof(FireBattleRifle));
            LoadoutAPI.AddSkill(typeof(ReloadBR));

            /*SkillDef primarySSGDef = SkillDef.CreateInstance<SkillDef>();
            primarySSGDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SuperShotgun));
            primarySSGDef.activationStateMachineName = "Weapon";
            primarySSGDef.baseMaxStock = 1;
            primarySSGDef.baseRechargeInterval = 0f;
            primarySSGDef.beginSkillCooldownOnSkillEnd = false;
            primarySSGDef.canceledFromSprinting = false;
            primarySSGDef.dontAllowPastMaxStocks = true;
            primarySSGDef.forceSprintDuringState = false;
            primarySSGDef.fullRestockOnAssign = true;
            primarySSGDef.icon = iconPrimary;
            primarySSGDef.interruptPriority = InterruptPriority.Any;
            primarySSGDef.isBullets = true;
            primarySSGDef.isCombatSkill = true;
            primarySSGDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            primarySSGDef.mustKeyPress = false;
            primarySSGDef.noSprint = false;
            primarySSGDef.rechargeStock = 0;
            primarySSGDef.requiredStock = 1;
            primarySSGDef.shootDelay = 0f;
            primarySSGDef.skillName = "SuperShotgun";
            primarySSGDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT_NAME";
            primarySSGDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION";
            primarySSGDef.stockToConsume = 1;
            ReloadBattleRifle.reloadIcon = iconReload;
            LoadoutAPI.AddSkillDef(primarySSGDef);
            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primarySSGDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySSGDef.skillNameToken, false)
            };*/
            //LoadoutAPI.AddSkill(typeof(EntityStates.SniperClassicSkills.SuperShotgun));
            //LoadoutAPI.AddSkill(typeof(EntityStates.SniperClassicSkills.ReloadSuperShotgun));
            


            LoadoutAPI.AddSkillFamily(primarySkillFamily);
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
            secondaryScopeDef.baseRechargeInterval = 6f;
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
            if (SecondaryScope.toggleScope)
            {
                secondaryScopeDef.mustKeyPress = true;
            }
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
            //SecondaryScope.scopeCrosshairPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/crosshair/snipercrosshair"), "SniperClassicScopeCrosshair", false);
            //DisplayStock ds = SecondaryScope.scopeCrosshairPrefab.GetComponent<DisplayStock>();
            //ds.skillSlot = SkillSlot.Secondary;
            //Destroy(EntityStates.SniperClassicSkills.SecondaryScope.crosshairPrefab.GetComponent<DisplayStock>());

            SecondaryScope.scopeCrosshairPrefab = scopeCrosshair;
            scopeCrosshair.AddComponent<HudElement>();
            CrosshairController cc = scopeCrosshair.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 0;
            scopeCrosshair.AddComponent<ScopeChargeIndicatorController>();

            SecondaryScope.noscopeCrosshairPrefab = noscopeCrosshair;
            noscopeCrosshair.AddComponent<HudElement>();
            cc = noscopeCrosshair.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 0;
            noscopeCrosshair.AddComponent<ScopeChargeIndicatorController>();

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
            utilityRollDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.CombatRoll2));
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
            LoadoutAPI.AddSkill(typeof(CombatRoll2));
            LoadoutAPI.AddSkillDef(utilityRollDef);
            LoadoutAPI.AddSkillFamily(utilitySkillFamily);
            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = utilityRollDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityRollDef.skillNameToken, false)
            };

            /*SkillDef utilitySmokeDef = SkillDef.CreateInstance<SkillDef>();
            utilitySmokeDef.activationState = new SerializableEntityStateType(typeof(AimSmokeGrenade));
            utilitySmokeDef.activationStateMachineName = "Weapon";
            utilitySmokeDef.baseMaxStock = 1;
            utilitySmokeDef.baseRechargeInterval = 14f;
            utilitySmokeDef.beginSkillCooldownOnSkillEnd = false;
            utilitySmokeDef.canceledFromSprinting = false;
            utilitySmokeDef.dontAllowPastMaxStocks = true;
            utilitySmokeDef.forceSprintDuringState = false;
            utilitySmokeDef.fullRestockOnAssign = true;
            utilitySmokeDef.icon = iconUtilitySmoke;
            utilitySmokeDef.interruptPriority = InterruptPriority.Any;
            utilitySmokeDef.isBullets = false;
            utilitySmokeDef.isCombatSkill = false;
            utilitySmokeDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_INVIS" };
            utilitySmokeDef.mustKeyPress = false;
            utilitySmokeDef.noSprint = true;
            utilitySmokeDef.rechargeStock = 1;
            utilitySmokeDef.requiredStock = 1;
            utilitySmokeDef.shootDelay = 0f;
            utilitySmokeDef.skillName = "SmokeGrenade";
            utilitySmokeDef.skillNameToken = "SNIPERCLASSIC_UTILITY_ALT_NAME";
            utilitySmokeDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_ALT_DESCRIPTION";
            utilitySmokeDef.stockToConsume = 1;
            LoadoutAPI.AddSkillDef(utilitySmokeDef);
            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = utilitySmokeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilitySmokeDef.skillNameToken, false)
            };
            LoadoutAPI.AddSkill(typeof(AimSmokeGrenade));
            LoadoutAPI.AddSkill(typeof(FireSmokeGrenade));*/
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
            GameObject spotterObject = Resources.Load<GameObject>(mdlSpotterPath);
            spotterObject.AddComponent<SpotterFollowerController>();
            ClientScene.RegisterPrefab(spotterObject);
            SpotterTargetingController.spotterFollowerGameObject = spotterObject;

            Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");
            spotterObject.GetComponent<Renderer>().material.shader = hotpoo;
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

        /*public void BuildGrenadePrefab()    //Credits to EnforcerGang for this code
        {
            GameObject smokeGrenadeModel = Resources.Load<GameObject>(smokeGrenadePath);
            GameObject smokeEffectPrefab = Resources.Load<GameObject>(smokeEffectPath);

           Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");
            smokeGrenadeModel.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;
            smokeEffectPrefab.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;

            //smokeEffectPrefab.GetComponentInChildren<Rigidbody>().gameObject.layer = LayerIndex.debris.intVal;

            GameObject smokeProjectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile").InstantiateClone("SniperClassicSmokeGrenade", true);
            GameObject smokeGasPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone").InstantiateClone("SniperClassicSmokeGrenadesDotZone", true);

            ProjectileController grenadeController = smokeProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileController tearGasController = smokeGasPrefab.GetComponent<ProjectileController>();

            ProjectileDamage grenadeDamage = smokeProjectilePrefab.GetComponent<ProjectileDamage>();
            ProjectileDamage gasDamage = smokeGasPrefab.GetComponent<ProjectileDamage>();


            TeamFilter filter = smokeGasPrefab.GetComponent<TeamFilter>();

            ProjectileImpactExplosion grenadeImpact = smokeProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            Destroy(smokeGasPrefab.GetComponent<ProjectileDotZone>());

            BuffWard buffWard = smokeGasPrefab.AddComponent<BuffWard>();
            BuffWard debuffWard = smokeGasPrefab.AddComponent<BuffWard>();

            filter.teamIndex = TeamIndex.Player;

            GameObject grenadeModel = smokeGrenadeModel.InstantiateClone("SniperClassicSmokeGrenadeGhost", true);
            grenadeModel.AddComponent<NetworkIdentity>();
            grenadeModel.AddComponent<ProjectileGhostController>();

            grenadeController.ghostPrefab = grenadeModel;

            grenadeImpact.lifetimeExpiredSoundString = "";
            grenadeImpact.explosionSoundString = "";
            grenadeImpact.offsetForLifetimeExpiredSound = 1;
            grenadeImpact.destroyOnEnemy = false;
            grenadeImpact.destroyOnWorld = true;
            grenadeImpact.timerAfterImpact = false;
            grenadeImpact.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            grenadeImpact.lifetime = 18;
            grenadeImpact.lifetimeAfterImpact = 0.5f;
            grenadeImpact.lifetimeRandomOffset = 0;
            grenadeImpact.blastRadius = 12;
            grenadeImpact.blastDamageCoefficient = 1;
            grenadeImpact.blastProcCoefficient = 1;
            grenadeImpact.fireChildren = true;
            grenadeImpact.childrenCount = 1;
            grenadeImpact.childrenProjectilePrefab = smokeGasPrefab;
            grenadeImpact.childrenDamageCoefficient = 0;
            grenadeImpact.impactEffect = null;

            grenadeController.startSound = "";
            grenadeController.procCoefficient = 1;
            tearGasController.procCoefficient = 0;

            grenadeDamage.crit = false;
            grenadeDamage.damage = 0f;
            grenadeDamage.damageColorIndex = DamageColorIndex.Default;
            grenadeDamage.damageType = DamageType.Stun1s;
            grenadeDamage.force = 0;

            gasDamage.crit = false;
            gasDamage.damage = 0;
            gasDamage.damageColorIndex = DamageColorIndex.WeakPoint;
            gasDamage.damageType = DamageType.Stun1s;
            gasDamage.force = -1000;

            buffWard.radius = 15;
            buffWard.interval = 0.2f;
            buffWard.rangeIndicator = Resources.Load<GameObject>("Prefabs/NetworkedObjects/WarbannerWard").GetComponent<BuffWard>().rangeIndicator;
            buffWard.buffType = BuffIndex.Cloak;
            buffWard.buffDuration = 0.3f;
            buffWard.floorWard = false;
            buffWard.expires = false;
            buffWard.invertTeamFilter = false;
            buffWard.expireDuration = 0;
            buffWard.animateRadius = false;

            debuffWard.radius = buffWard.radius;
            debuffWard.interval = 1f;
            debuffWard.rangeIndicator = null;
            debuffWard.buffType = BuffIndex.Slow50;
            debuffWard.buffDuration = 2f;
            debuffWard.floorWard = false;
            debuffWard.invertTeamFilter = true;
            debuffWard.expireDuration = 0;
            debuffWard.animateRadius = false;

            Destroy(smokeGasPrefab.transform.GetChild(0).gameObject);
            GameObject gasFX = smokeEffectPrefab.InstantiateClone("FX", true);
            gasFX.AddComponent<NetworkIdentity>();
            gasFX.AddComponent<SmokeComponent>();
            gasFX.transform.parent = smokeGasPrefab.transform;
            gasFX.transform.localPosition = Vector3.zero;

            smokeGasPrefab.AddComponent<DestroyOnTimer>().duration = 4;

            FireSmokeGrenade.projectilePrefab = smokeProjectilePrefab;
        }*/

        public void ReadConfig()
        {
            ConfigEntry<bool> scopeToggle = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Toggle Scope"), false, new ConfigDescription("Makes Steady Aim not require you to hold down the skill key to use."));
            ConfigEntry<float> scopeZoomFOV = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Default FOV"), 80f, new ConfigDescription("Default zoom level of Steady Aim (accepts values from 5-50)."));
            ConfigEntry<bool> scopeResetZoom = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Reset Zoom on Unscope"), false, new ConfigDescription("Reset scope zoom level when unscoping."));
            ConfigEntry<bool> scopeUseScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Use Scroll Wheel for Zoom"), true, new ConfigDescription("Scroll wheel changes zoom level. Scroll up to zoom in, scroll down to zoom out."));
            ConfigEntry<bool> scopeInvertScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Invert Scroll Wheel"), false, new ConfigDescription("Reverses scroll wheel direction. Scroll up to zoom out, scroll down to zoom in."));
            ConfigEntry<float> scopeScrollZoomSpeed = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Scroll Wheel Zoom Speed"), 20f, new ConfigDescription("Zoom speed when using the scroll wheel."));
            ConfigEntry<KeyCode> scopeZoomInKey = base.Config.Bind<KeyCode>(new ConfigDefinition("20 - Secondary - Steady Aim", "Zoom-In Button"), KeyCode.None, new ConfigDescription("Keyboard button that zooms the scope in."));
            ConfigEntry<KeyCode> scopeZoomOutKey = base.Config.Bind<KeyCode>(new ConfigDefinition("20 - Secondary - Steady Aim", "Zoom-Out Button"), KeyCode.None, new ConfigDescription("Keyboard button that zooms the scope out."));
            ConfigEntry<float> scopeButtonZoomSpeed = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Button Zoom Speed"), 1f, new ConfigDescription("Zoom speed when using keyboard buttons."));
            SecondaryScope.zoomFOV = scopeZoomFOV.Value;
            if (SecondaryScope.zoomFOV < SecondaryScope.minFOV)
            {
                SecondaryScope.zoomFOV = SecondaryScope.minFOV;
            }
            else if (SecondaryScope.zoomFOV > SecondaryScope.maxFOV)
            {
                SecondaryScope.zoomFOV = SecondaryScope.maxFOV;
            }
            SecondaryScope.useScrollWheelZoom = scopeUseScrollWheel.Value;
            SecondaryScope.invertScrollWheelZoom = scopeInvertScrollWheel.Value;
            SecondaryScope.zoomInKey = scopeZoomInKey.Value;
            SecondaryScope.zoomOutKey = scopeZoomOutKey.Value;
            SecondaryScope.scrollZoomSpeed = scopeScrollZoomSpeed.Value;
            SecondaryScope.buttonZoomSpeed = scopeButtonZoomSpeed.Value;
            SecondaryScope.resetZoom = scopeResetZoom.Value;
            SecondaryScope.toggleScope = scopeToggle.Value;
        }

        public void LoadResources()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SniperClassic.sniperbundle"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new R2API.AssetBundleResourcesProvider(assetPrefix, bundle);
                R2API.ResourcesAPI.AddProvider(provider);
            }
            sniperIcon = Resources.Load<Texture2D>(portraitPath);
            ReloadController.reloadBar = Resources.Load<Texture2D>(textureBarPath);
            ReloadController.reloadCursor = Resources.Load<Texture2D>(textureCursorPath);
            ReloadController.reloadBarFail = Resources.Load<Texture2D>(textureBarFailPath);
            ReloadController.reloadCursorFail = Resources.Load<Texture2D>(textureCursorFailPath);
            ReloadController.indicatorGood = Resources.Load<Texture2D>(textureReloadGoodPath);
            ReloadController.indicatorPerfect = Resources.Load<Texture2D>(textureReloadPerfectPath);
            ScopeController.stockEmpty = Resources.Load<Texture2D>(textureReloadEmptyPath);
            ScopeController.stockAvailable = ReloadController.indicatorGood;
            iconSpecialReturn = Resources.Load<Sprite>(textureIconSpecialReturnPath);
            iconReload = Resources.Load<Sprite>(textureIconReloadPath);
            iconPrimary = Resources.Load<Sprite>(textureIconPrimaryPath);
            iconSecondary = Resources.Load<Sprite>(textureIconSecondaryPath);
            iconUtility = Resources.Load<Sprite>(textureIconUtilityPath);
            //iconUtilitySmoke = Resources.Load<Sprite>(textureIconUtilitySmokePath);
            iconSpecial = Resources.Load<Sprite>(textureIconSpecialPath);
            noscopeCrosshair = Resources.Load<GameObject>(noscopeCrosshairPath);
            scopeCrosshair = Resources.Load<GameObject>(scopeCrosshairPath);
            iconPrimaryAlt = Resources.Load<Sprite>(textureIconPrimaryAltPath);

            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SniperClassic.SniperClassic_Sounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }
        }
    }
}
