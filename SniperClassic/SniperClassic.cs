using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Bison;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.SniperClassicSkills;
using KinematicCharacterController;
using R2API;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
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
    [BepInPlugin("com.Moffein.SniperClassic", "Sniper Classic", "0.4.7")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(PrefabAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(BuffAPI), nameof(EffectAPI), nameof(SoundAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    
    public class SniperClassic : BaseUnityPlugin
    {
        Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");
        public static GameObject SniperBody = null;
        GameObject SniperDisplay = null;
        Sprite iconPrimary = null;
        Sprite iconPrimaryHeavy = null;
        Sprite iconSecondary = null;
        Sprite iconUtility = null;
        Sprite iconSpecial = null;
        Sprite iconSpecialReturn = null;
        Sprite iconReload = null;
        Sprite iconPrimaryAlt = null;
        Color SniperColor = new Color(78f / 255f, 80f / 255f, 111f / 255f);
        public const string assetPrefix = "@MoffeinSniperClassic";
        const string portraitPath = assetPrefix + ":texSniperIcon.png";
        const string textureBarPath = assetPrefix + ":texReloadBar.png";
        const string textureCursorPath = assetPrefix + ":texReloadSlider.png";
        const string textureBarFailPath = assetPrefix + ":texReloadBarFail.png";
        const string textureCursorFailPath = assetPrefix + ":texReloadSliderFail.png";
        const string textureReloadEmptyPath = assetPrefix + ":texReloadEmpty.png";
        const string textureReloadGoodPath = assetPrefix + ":texReloadGood.png";
        const string textureReloadPerfectPath = assetPrefix + ":texReloadPerfect.png";
        const string textureIconSpecialReturnPath = assetPrefix + ":texSpecialCancelIcon.png";
        const string textureIconReloadPath = assetPrefix + ":texPrimaryReloadIcon.png";
        const string textureIconPrimaryPath = assetPrefix + ":texPrimaryIcon.png";
        const string textureIconPrimaryAltPath = assetPrefix + ":texPrimaryAltIcon.png";
        const string textureIconPrimaryHeavyPath = assetPrefix + ":texPrimaryAlt2Icon.png";
        const string textureIconSecondaryPath = assetPrefix + ":texSecondaryIcon.png";
        const string textureIconUtilityPath = assetPrefix + ":texUtilityIcon.png";
        const string textureIconSpecialPath = assetPrefix + ":texSpecialIcon.png";
        const string mdlSpotterPath = assetPrefix + ":mdlSpotter.prefab";
        const string noscopeCrosshairPath = assetPrefix + ":NoscopeCrosshair.prefab";
        const string scopeCrosshairPath = assetPrefix + ":ScopeCrosshair.prefab";

        GameObject noscopeCrosshair = null;
        GameObject scopeCrosshair = null;

        Texture2D sniperIcon = null;

        public static BuffIndex spotterStatDebuff;
        public static BuffIndex spotterBuff;
        public static BuffIndex spotterCooldownBuff;

        SkillDef spotDef, spotReturnDef;

        public void Awake()
        {
            Setup();

            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                if (self.HasBuff(spotterStatDebuff))
                {
                    self.armor -= 20f;
                    self.moveSpeed *= 0.6f;
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
                            if (damageInfo.procCoefficient > 0f && !(damageInfo.damage / attackerBody.damage < 4f))
                            {

                                if (victimBody && victimBody.healthComponent && victimBody.healthComponent.alive)
                                {
                                    victimBody.RemoveBuff(spotterBuff);
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        victimBody.AddTimedBuff(spotterCooldownBuff, i);
                                    }
                                }

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

                                SpotterLightningController stc = damageInfo.attacker.GetComponent<SpotterLightningController>();
                                if (stc)
                                {
                                    stc.QueueLightning(spotterLightning, 0.1f);
                                }

                                /*new BlastAttack
                                {
                                    attacker = damageInfo.attacker,
                                    inflictor = damageInfo.attacker,
                                    teamIndex = attackerBody.teamComponent.teamIndex,
                                    baseDamage = damageInfo.damage * 0.3f,
                                    baseForce = 0f,
                                    position = damageInfo.position,
                                    radius = 20f,
                                    procCoefficient = 0.33f,
                                    falloffModel = BlastAttack.FalloffModel.None,
                                    damageType = DamageType.Stun1s | DamageType.SlowOnHit,
                                    crit = damageInfo.crit,
                                    attackerFiltering = AttackerFiltering.NeverHit
                                }.Fire();

                                EffectManager.SpawnEffect(shockExplosionEffect, new EffectData
                                {
                                    origin = damageInfo.position,
                                    scale = 20f
                                }, true);*/
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
            CreatePrefab();
            //SetupBody();
            CreateDisplayPrefab();
            SetupStats();
            SetupEffects();
            AddSkin();
            AssignSkills();
            RegisterSurvivor();
            RegisterLanguageTokens();
            Modules.ItemDisplays.RegisterDisplays();
            CreateMaster();
        }

        private void SetupEffects()
        {
            CreateSpotterLightningEffect();
            FixTracer();

            void CreateSpotterLightningEffect()
            {
                SpotterLightningController.shockExplosionEffect = Resources.Load<GameObject>("prefabs/effects/lightningstakenova").InstantiateClone("MoffeinSniperClassicSpotterLightningExplosion",false);
                EffectComponent ec = SpotterLightningController.shockExplosionEffect.GetComponent<EffectComponent>();
                ec.applyScale = true;
                ec.soundName = "Play_mage_m2_impact";
                EffectAPI.AddEffect(SpotterLightningController.shockExplosionEffect);
            }

            void FixTracer()
            {
                GameObject sniperTracerObject = Resources.Load<GameObject>("prefabs/effects/tracers/tracersmokechase");
                DestroyOnTimer destroyTimer = sniperTracerObject.AddComponent<DestroyOnTimer>();
                destroyTimer.duration = 0.42f;

                Snipe.tracerEffectPrefab = sniperTracerObject;
                HeavySnipe.tracerEffectPrefab = sniperTracerObject;
                FireBattleRifle.tracerEffectPrefab = sniperTracerObject;
            }
        }

        private GameObject CreateBodyModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            return Resources.Load<GameObject>(assetPrefix + ":mdlSniper.prefab");
        }

        private void CreatePrefab()
        {
            #region add all the things
            GameObject characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "SniperClassicBody", true);

            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject model = CreateBodyModel(characterPrefab);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = characterPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.91f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();

            cameraTargetParams.cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraTargetParams.cameraParams.maxPitch = 70;
            cameraTargetParams.cameraParams.minPitch = -70;
            cameraTargetParams.cameraParams.wallCushion = 0.1f;
            cameraTargetParams.cameraParams.pivotVerticalOffset = 1.7f;
            cameraTargetParams.cameraParams.standardLocalCameraPos = new Vector3(0, 0f, -8.2f);

            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            Material sniperMat = Modules.Skins.CreateMaterial("matSniper.mat", 1f, Color.white);

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = characterPrefab.GetComponent<CharacterBody>();
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperMat,
                    renderer = childLocator.FindChild("GunModel").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperMat,
                    renderer = childLocator.FindChild("Model").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            //characterModel.baseRendererInfos[0].defaultMaterial.shader = hotpoo;
            //characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            /*characterModel.baseRendererInfos[0].defaultMaterial.SetTexture("_EmTex", Assets.mainMat.GetTexture("_EmissionMap"));
            characterModel.baseRendererInfos[0].defaultMaterial.SetFloat("_EmPower", 1f);
            characterModel.baseRendererInfos[0].defaultMaterial.SetColor("_EmColor", Color.white);*/

            TeamComponent teamComponent = null;
            if (characterPrefab.GetComponent<TeamComponent>() != null) teamComponent = characterPrefab.GetComponent<TeamComponent>();
            else teamComponent = characterPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 100f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            //characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = characterPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox mainHurtbox = model.transform.Find("MainHurtbox").GetComponent<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = healthComponent;
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                mainHurtbox
            };

            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

            /*RagdollController ragdollController = model.GetComponent<RagdollController>();
            PhysicMaterial physicMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;
            foreach (Transform i in ragdollController.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider j = i.GetComponent<Collider>();
                    if (j) 
                    {
                        j.material = physicMat;
                        j.sharedMaterial = physicMat;
                    }
                }
            }*/

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -90f;
            aimAnimator.yawRangeMax = 90f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = characterPrefab.GetComponent<InputBankTest>();
            #endregion

            SniperBody = characterPrefab;
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(SniperBody);
            };
        }

        private void CreateDisplayPrefab()
        {
            GameObject model = Resources.Load<GameObject>(assetPrefix + ":SniperDisplay.prefab");

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            Material sniperMat = Modules.Skins.CreateMaterial("matSniper", 1f, Color.white);

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = null;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperMat,
                    renderer = childLocator.FindChild("GunModel").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperMat,
                    renderer = childLocator.FindChild("Model").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            /*characterModel.baseRendererInfos[0].defaultMaterial.SetTexture("_EmTex", Assets.mainMat.GetTexture("_EmissionMap"));
            characterModel.baseRendererInfos[0].defaultMaterial.SetFloat("_EmPower", 1f);*/

            SniperDisplay = PrefabAPI.InstantiateClone(model, "SniperClassicDisplay", false);

            /*CharacterSelectSurvivorPreviewDisplayController displayController = SniperDisplay.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            displayController.bodyPrefab = SniperBody;*/
        }

        public void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SNIPERCLASSIC_BODY_NAME", "Sniper");
            LanguageAPI.Add("SNIPERCLASSIC_BODY_SUBTITLE", "Eagle Eye");
            LanguageAPI.Add("SNIPERCLASSIC_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_NAME", "Snipe");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_DESCRIPTION", "<style=cIsUtility>Agile</style>. Fire a piercing shot for <style=cIsDamage>360% damage</style>. After firing, <style=cIsDamage>reload your weapon</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly.");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_NAME", "Mark");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>300% damage</style>. After emptying your clip, <style=cIsDamage>reload your weapon</style> and <style=cIsUtility>gain 1 charge of Steady Aim</style> if perfectly timed.");

            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_NAME", "Heavy Snipe");
            LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION", "<style=cIsUtility>Agile</style>. Fire a piercing shot for <style=cIsDamage>480% damage</style>. After firing, <style=cIsDamage>reload your weapon</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly." +
                " <style=cIsHealth>-25% TOTAL damage while scoped</style>.");


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
            LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style> with your Spotter, reducing their movement speed and armor. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to transfer <style=cIsDamage>40% TOTAL damage</style> to all enemies near your Spotter.");

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
                SniperBody.AddComponent<SpotterLightningController>();
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
                    cb.skinIndex = 0u;
                }
            }
        }

        public void AddSkin()    //credits to rob
        {
            //do Modules.Skins.AddSkins() or something, it's got some helpers to make skin setup easier
            //clean up this spaghetti
            GameObject bodyPrefab = SniperBody;
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = null;
            if (model.GetComponent<ModelSkinController>())
                skinController = model.GetComponent<ModelSkinController>();
            else
                skinController = model.AddComponent<ModelSkinController>();

            /*SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");
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
            }*/

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f/255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), SniperColor);
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    //renderer = mainRenderer,
                    //mesh = mainRenderer.sharedMesh
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
            primaryBRDef.icon = iconPrimaryAlt;
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
            primaryHeavySnipeDef.icon = iconPrimaryHeavy;
            primaryHeavySnipeDef.interruptPriority = InterruptPriority.Any;
            primaryHeavySnipeDef.isBullets = true;
            primaryHeavySnipeDef.isCombatSkill = true;
            primaryHeavySnipeDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            primaryHeavySnipeDef.mustKeyPress = true;
            primaryHeavySnipeDef.noSprint = false;
            primaryHeavySnipeDef.rechargeStock = 1;
            primaryHeavySnipeDef.requiredStock = 1;
            primaryHeavySnipeDef.shootDelay = 0f;
            primaryHeavySnipeDef.skillName = "HeavySnipe";
            primaryHeavySnipeDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT2_NAME";
            primaryHeavySnipeDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION";
            primaryHeavySnipeDef.stockToConsume = 1;
            LoadoutAPI.AddSkill(typeof(HeavySnipe));
            LoadoutAPI.AddSkillDef(primaryHeavySnipeDef);
            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primaryHeavySnipeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryHeavySnipeDef.skillNameToken, false)
            };

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
            if (SecondaryScope.toggleScope || SecondaryScope.csgoZoom)
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

            spotDef = specialSpotDef;

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

            spotReturnDef = specialSpotReturnDef;

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
            spotterObject.GetComponentInChildren<MeshRenderer>().material = Modules.Skins.CreateMaterial("matSniper", 1f, Color.white);
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

        public void ReadConfig()
        {
            ConfigEntry<bool> scopeCSGOZoom = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Preset Zoom (Overrides all other settings)"), false, new ConfigDescription("Pressing M2 cycles through preset zoom levels."));
            ConfigEntry<bool> scopeToggle = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Toggle Scope"), false, new ConfigDescription("Makes Steady Aim not require you to hold down the skill key to use."));
            ConfigEntry<float> scopeZoomFOV = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Default FOV"), 50f, new ConfigDescription("Default zoom level of Steady Aim (accepts values from 5-50)."));
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
            SecondaryScope.csgoZoom = scopeCSGOZoom.Value;
        }

        public void LoadResources()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SniperClassic.sniperclassic"))
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
            iconPrimaryHeavy = Resources.Load<Sprite>(textureIconPrimaryHeavyPath);
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
                SoundAPI.SoundBanks.Add(bytes);
            }
        }

        private void CreateMaster()
        {
            GameObject SniperMonsterMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "SniperClassicMonsterMaster", true);
            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(SniperMonsterMaster);
            };

            CharacterMaster cm = SniperMonsterMaster.GetComponent<CharacterMaster>();
            cm.bodyPrefab = SniperBody;

            Component[] toDelete = SniperMonsterMaster.GetComponents<AISkillDriver>();
            foreach (AISkillDriver asd in toDelete)
            {
                Destroy(asd);
            }

            AISkillDriver sendSpotter = SniperMonsterMaster.AddComponent<AISkillDriver>();
            sendSpotter.skillSlot = SkillSlot.Special;
            sendSpotter.requiredSkill = spotDef;
            sendSpotter.requireSkillReady = true;
            sendSpotter.requireEquipmentReady = false;
            sendSpotter.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            sendSpotter.minDistance = 0f;
            sendSpotter.maxDistance = 60f;
            sendSpotter.maxDistance = float.PositiveInfinity;
            sendSpotter.selectionRequiresTargetLoS = true;
            sendSpotter.activationRequiresTargetLoS = true;
            sendSpotter.activationRequiresAimConfirmation = true;
            sendSpotter.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            sendSpotter.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            sendSpotter.ignoreNodeGraph = false;
            sendSpotter.driverUpdateTimerOverride = 0.2f;
            sendSpotter.noRepeat = true;
            sendSpotter.shouldSprint = false;
            sendSpotter.shouldFireEquipment = false;
            sendSpotter.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            AISkillDriver returnSpotterDistance = SniperMonsterMaster.AddComponent<AISkillDriver>();
            returnSpotterDistance.skillSlot = SkillSlot.Special;
            returnSpotterDistance.requiredSkill = spotReturnDef;
            returnSpotterDistance.requireSkillReady = true;
            returnSpotterDistance.requireEquipmentReady = false;
            returnSpotterDistance.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            returnSpotterDistance.minDistance = 65f;
            returnSpotterDistance.maxDistance = float.PositiveInfinity;
            returnSpotterDistance.selectionRequiresTargetLoS = false;
            returnSpotterDistance.activationRequiresTargetLoS = false;
            returnSpotterDistance.activationRequiresAimConfirmation = false;
            returnSpotterDistance.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            returnSpotterDistance.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            returnSpotterDistance.ignoreNodeGraph = false;
            returnSpotterDistance.driverUpdateTimerOverride = 0.2f;
            returnSpotterDistance.noRepeat = true;
            returnSpotterDistance.shouldSprint = false;
            returnSpotterDistance.shouldFireEquipment = false;
            returnSpotterDistance.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            AISkillDriver roll = SniperMonsterMaster.AddComponent<AISkillDriver>();
            roll.skillSlot = SkillSlot.Utility;
            roll.requireSkillReady = true;
            roll.requireEquipmentReady = false;
            roll.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            roll.minDistance = 0f;
            roll.maxDistance = float.PositiveInfinity;
            roll.selectionRequiresTargetLoS = false;
            roll.activationRequiresTargetLoS = false;
            roll.activationRequiresAimConfirmation = false;
            roll.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            roll.aimType = AISkillDriver.AimType.AtMoveTarget;
            roll.ignoreNodeGraph = false;
            roll.driverUpdateTimerOverride = -1f;
            roll.noRepeat = true;
            roll.shouldSprint = true;
            roll.shouldFireEquipment = false;
            roll.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            AISkillDriver scopeAggressive = SniperMonsterMaster.AddComponent<AISkillDriver>();
            scopeAggressive.skillSlot = SkillSlot.Secondary;
            scopeAggressive.requireSkillReady = true;
            scopeAggressive.requireEquipmentReady = false;
            scopeAggressive.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scopeAggressive.minDistance = 100f;
            scopeAggressive.maxDistance = float.PositiveInfinity;
            scopeAggressive.minUserHealthFraction = 0.7f;
            scopeAggressive.selectionRequiresTargetLoS = true;
            scopeAggressive.activationRequiresTargetLoS = true;
            scopeAggressive.activationRequiresAimConfirmation = true;
            scopeAggressive.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            scopeAggressive.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scopeAggressive.ignoreNodeGraph = false;
            scopeAggressive.driverUpdateTimerOverride = 2f;
            scopeAggressive.noRepeat = true;
            scopeAggressive.shouldSprint = false;
            scopeAggressive.shouldFireEquipment = false;
            scopeAggressive.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            AISkillDriver scope = SniperMonsterMaster.AddComponent<AISkillDriver>();
            scope.skillSlot = SkillSlot.Secondary;
            scope.requireSkillReady = true;
            scope.requireEquipmentReady = false;
            scope.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scope.minDistance = 120f;
            scope.maxDistance = float.PositiveInfinity;
            scope.minUserHealthFraction = 0.25f;
            scopeAggressive.maxUserHealthFraction = 0.7f;
            scope.selectionRequiresTargetLoS = true;
            scope.activationRequiresTargetLoS = true;
            scope.activationRequiresAimConfirmation = true;
            scope.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            scope.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scope.ignoreNodeGraph = false;
            scope.driverUpdateTimerOverride = 2f;
            scope.noRepeat = true;
            scope.shouldSprint = false;
            scope.shouldFireEquipment = false;
            scope.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            AISkillDriver strafeShoot = SniperMonsterMaster.AddComponent<AISkillDriver>();
            strafeShoot.skillSlot = SkillSlot.Primary;
            strafeShoot.requireSkillReady = false;
            strafeShoot.requireEquipmentReady = false;
            strafeShoot.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeShoot.minDistance = 0f;
            strafeShoot.maxDistance = float.PositiveInfinity;
            strafeShoot.selectionRequiresTargetLoS = true;
            strafeShoot.activationRequiresTargetLoS = true;
            strafeShoot.activationRequiresAimConfirmation = true;
            strafeShoot.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeShoot.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            strafeShoot.ignoreNodeGraph = false;
            strafeShoot.driverUpdateTimerOverride = 0.8f;
            strafeShoot.noRepeat = false;
            strafeShoot.shouldSprint = false;
            strafeShoot.shouldFireEquipment = false;
            strafeShoot.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            AISkillDriver afk = SniperMonsterMaster.AddComponent<AISkillDriver>();
            afk.skillSlot = SkillSlot.None;
            afk.requireSkillReady = false;
            afk.requireEquipmentReady = false;
            afk.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            afk.minDistance = 0f;
            afk.maxDistance = float.PositiveInfinity;
            afk.selectionRequiresTargetLoS = false;
            afk.activationRequiresTargetLoS = false;
            afk.activationRequiresAimConfirmation = false;
            afk.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk.aimType = AISkillDriver.AimType.MoveDirection;
            afk.ignoreNodeGraph = false;
            afk.driverUpdateTimerOverride = -1f;
            afk.noRepeat = false;
            afk.shouldSprint = true;
            afk.shouldFireEquipment = false;
            afk.shouldTapButton = false;

            AISkillDriver afk2 = SniperMonsterMaster.AddComponent<AISkillDriver>();
            afk2.skillSlot = SkillSlot.None;
            afk2.requireSkillReady = false;
            afk2.requireEquipmentReady = false;
            afk2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            afk2.minDistance = 0f;
            afk2.maxDistance = float.PositiveInfinity;
            afk2.selectionRequiresTargetLoS = false;
            afk2.activationRequiresTargetLoS = false;
            afk2.activationRequiresAimConfirmation = false;
            afk2.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk2.aimType = AISkillDriver.AimType.MoveDirection;
            afk2.ignoreNodeGraph = false;
            afk2.driverUpdateTimerOverride = -1f;
            afk2.noRepeat = false;
            afk2.shouldSprint = true;
            afk2.shouldFireEquipment = false;
            afk2.shouldTapButton = false;
        }
    }
}
