using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.SniperClassicSkills;
using KinematicCharacterController;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using SniperClassic.Controllers;
using SniperClassic.Controllers.SmokeGrenade;
using SniperClassic.Hooks;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    [BepInDependency("com.bepis.r2api")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(RecalculateStatsAPI))]
    [BepInDependency("com.Kingpinush.KingKombatArena", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.SniperClassic", "Sniper Classic", "0.8.0")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    public class SniperClassic : BaseUnityPlugin
    {
        readonly Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");
        public static GameObject SniperBody = null;
        GameObject SniperDisplay = null;
        public static Color SniperColor = new Color(78f / 255f, 80f / 255f, 111f / 255f);

        public static bool arenaNerf = true;
        public static bool arenaPluginLoaded = false;
        public static bool arenaActive = false;

        SkillDef spotDef, spotReturnDef, spotScepterDef, scopeDef;

        public void Awake()
        {
            Setup();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
            {
                SetupScepter();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Kingpinush.KingKombatArena") && arenaNerf)
            {
                arenaPluginLoaded = true;
            }
            AddHooks();
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }
        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new SniperContent());
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void SetupScepter()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(spotScepterDef, "SniperClassicBody", SkillSlot.Special, 0);
        }

        public void Start()
        {
            Modules.ItemDisplays.RegisterDisplays();
        }

        public void Setup()
        {
            LoadResources();
            Modules.Assets.InitializeAssets();
            ReadConfig();
            CreatePrefab();
            CreateDisplayPrefab();
            SetupStats();
            SetupEffects();
            AddSkin();
            AssignSkills();
            RegisterSurvivor();
            RegisterLanguageTokens();
            CreateMaster();

            SetupProjectiles();
        }

        private void SetupProjectiles()
        {
            SetupNeedleRifleProjectile();
            SetupSmokeGrenade();
            SetupHeavySnipeProjectile();
        }

        private void AddHooks()
        {
            RecalculateStats.AddHook();
            OnHitEnemy.AddHook();
            Stage_Start.AddHook();
        }

        private void SetupEffects()
        {
            CreateSpotterLightningEffect();
            FixTracer();

            void CreateSpotterLightningEffect()
            {
                SpotterLightningController.shockExplosionEffect = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinSniperClassicSpotterLightningExplosion", false);
                EffectComponent ec = SpotterLightningController.shockExplosionEffect.GetComponent<EffectComponent>();
                ec.applyScale = true;
                ec.soundName = "Play_mage_m2_impact";
                SniperContent.effectDefs.Add(new EffectDef(SpotterLightningController.shockExplosionEffect));
            }

            void FixTracer()
            {
                GameObject sniperTracerObject = Resources.Load<GameObject>("prefabs/effects/tracers/tracersmokechase");
                DestroyOnTimer destroyTimer = sniperTracerObject.AddComponent<DestroyOnTimer>();
                destroyTimer.duration = 0.42f;

                //Snipe.tracerEffectPrefab = sniperTracerObject;
                //HeavySnipe.tracerEffectPrefab = sniperTracerObject;
                FireBattleRifle.tracerEffectPrefab = sniperTracerObject;
            }
        }

        private GameObject CreateBodyModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            return SniperContent.assetBundle.LoadAsset<GameObject>("mdlSniper.prefab");
        }

        private void CreatePrefab()
        {
            #region add all the things
            GameObject characterPrefab = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "SniperClassicBody", true);

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
            gameObject3.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            CharacterBody cb = characterPrefab.GetComponent<CharacterBody>();
            cb.aimOriginTransform = gameObject3.transform;

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
            cameraTargetParams.cameraParams.pivotVerticalOffset = 0.5f;
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

            Material sniperMat = Modules.Skins.CreateMaterial("matSniper.mat", 0.7f, Color.white);
            Material sniperGunMat = Modules.Skins.CreateMaterial("matSniper.mat", 5f, new Color(192f / 255f, 152f / 255f, 216f / 255f));

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = characterPrefab.GetComponent<CharacterBody>();
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperGunMat,
                    renderer = childLocator.FindChild("GunModel").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperGunMat,
                    renderer = childLocator.FindChild("AltRifleModel").GetComponent<SkinnedMeshRenderer>(),
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

            CapsuleCollider hurtboxCapsule = model.transform.Find("MainHurtbox").GetComponent<CapsuleCollider>();
            hurtboxCapsule.radius = 0.28f;
            hurtboxCapsule.height = 2.1f;

            HurtBox mainHurtbox = hurtboxCapsule.gameObject.AddComponent<HurtBox>();
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

            RagdollController ragdollController = model.GetComponent<RagdollController>();
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
            }

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

            characterPrefab.AddComponent<Controllers.GunController>();
            #endregion

            SniperBody = characterPrefab;
            SniperContent.bodyPrefabs.Add(SniperBody);
        }

        private void CreateDisplayPrefab()
        {
            GameObject model = SniperContent.assetBundle.LoadAsset<GameObject>("SniperDisplay.prefab");

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            Material sniperMat = Modules.Skins.CreateMaterial("matSniper.mat", 0.7f, Color.white);
            Material sniperGunMat = Modules.Skins.CreateMaterial("matSniper.mat", 5f, new Color(192f / 255f, 152f / 255f, 216f / 255f));

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = null;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperGunMat,
                    renderer = childLocator.FindChild("GunModel").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperGunMat,
                    renderer = childLocator.FindChild("AltRifleModel").GetComponent<SkinnedMeshRenderer>(),
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
            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

            SniperDisplay = R2API.PrefabAPI.InstantiateClone(model, "SniperClassicDisplay", false);

            /*CharacterSelectSurvivorPreviewDisplayController displayController = SniperDisplay.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            displayController.bodyPrefab = SniperBody;*/
        }

        public void RegisterLanguageTokens()
        {
            R2API.LanguageAPI.Add("SNIPERCLASSIC_BODY_NAME", "Sniper");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_BODY_SUBTITLE", "Eagle Eye");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_DEFAULT_SKIN_NAME", "Default");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_NAME", "Snipe");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>360% damage</style>. After firing, <style=cIsDamage>reload</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_RELOAD_NAME", "Reload");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_RELOAD_DESCRIPTION", "Reload your weapon.");


            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_NAME", "Mark");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT_DESCRIPTION", "Fire a piercing shot for <style=cIsDamage>300% damage</style>. After emptying your clip, <style=cIsDamage>reload</style> and <style=cIsUtility>gain 1 Secondary charge</style> if perfectly timed.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_NAME", "Hard Impact");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION", "Fire a heavy artillery shell for <style=cIsDamage>360% damage</style>. After firing, <style=cIsDamage>reload</style> to gain up to <style=cIsDamage>1.5x bonus damage</style> if timed correctly.");


            R2API.LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_NAME", "Steady Aim");

            string secondaryDesc = "<style=cIsDamage>Stunning</style>. Carefully take aim, <style=cIsDamage>increasing the damage</style> of your next shot up to <style=cIsDamage>4.0x</style>.";
            if (SecondaryScope.useScrollWheelZoom)
            {
                secondaryDesc += " Use the scroll wheel to change zoom level.";
            }
            R2API.LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_DESCRIPTION", secondaryDesc);

            R2API.LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_ALT_NAME", "Trickshot");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_SECONDARY_ALT_DESCRIPTION", "<style=cIsDamage>Reloading</style>. Spin <style=cIsUtility>360</style> degrees, <style=cIsDamage>increasing the damage</style> of your next shot by <style=cIsDamage>2.5x</style>. <style=cIsHealth>WIP: NEEDS ANIMATION</style>.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_NAME", "Combat Training");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_DESCRIPTION", "<style=cIsDamage>Reloading</style>. <style=cIsUtility>Roll</style> a short distance.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_BACKFLIP_NAME", "Military Training");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION", "<style=cIsDamage>Reloading</style>. <style=cIsUtility>Backflip</style> into the air.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_SMOKE_NAME", "Smokescreen");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_SMOKE_DESCRIPTION", "Cover an area in smoke for 12 seconds, <style=cIsUtility>slowing</style> enemies and making all allies <style=cIsUtility>invisible</style>.");


            /*R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_ALT_NAME", "Smokescreen");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_UTILITY_ALT_DESCRIPTION", "Throw a smoke grenade that <style=cIsDamage>slows enemies</style> and conceals allies, making them <style=cIsUtility>invisible</style>.");*/

            //R2API.LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_INVIS", "<style=cKeywordName>Invisible</style><style=cSub>Enemies are unable to target you.</style>");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_NAME", "Spotter: FEEDBACK");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style> with your Spotter. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to zap nearby enemies for <style=cIsDamage>50% TOTAL damage</style>.");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_SCEPTER_NAME", "Spotter: OVERLOAD");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_SPECIAL_SCEPTER_DESCRIPTION", "<style=cIsDamage>Analyze an enemy</style> with your Spotter. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to zap nearby enemies for <style=cIsDamage>100% TOTAL damage</style>.");

            R2API.LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_ANALYZED", "<style=cKeywordName>Analyzed</style><style=cSub>Reduce movement speed by <style=cIsDamage>40%</style> and reduce armor by <style=cIsDamage>25</style>. Hit <style=cIsDamage>Analyzed</style> enemies for <style=cIsDamage>more than 400% damage</style> to deal <style=cIsDamage>50% TOTAL damage</style> to all enemies near your Spotter (Recharges every <style=cIsUtility>10</style> seconds).</style>");

            R2API.LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR", "..and so they left, the sound still ringing in deaf ears.");
            R2API.LanguageAPI.Add("SNIPERCLASSIC_OUTRO_FLAVOR_JOKE", "..and so they left, having never picked up a weel gun.");

            String sniperDesc = "";
            sniperDesc += "The Sniper is an marksman who works with his trusty Spotter drone to eliminate targets from afar.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Snipe must be reloaded after every shot. Learn the timing to maximize your damage output!" + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Sniper's reloads are unaffected by attack speed." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Military Training allows you to escape from danger while charging Steady Aim." + Environment.NewLine + Environment.NewLine;
            sniperDesc += "< ! > Steady Aim combined with Spotter: FEEDBACK and a perfectly reloaded Snipe can wipe out crowds of enemies." + Environment.NewLine + Environment.NewLine;
            R2API.LanguageAPI.Add("SNIPERCLASSIC_DESCRIPTION", sniperDesc);

            String tldr = "bio: sniper was born with a special power he was stronger than all his crewmates at the ues contact light. he served in the risk of rain military fighting providence and in the final battel against providence they were fighting and providence turned him to the darkness and sniper turned against HAN-D and kill him preventing him from being in the sequel. he killed his own spotter drone in the battle which is why it isn't here please stop PMing asking me why that's why. also capes are cool fuck you space_cowboy226 everyone knows youre a red item stealing faggot\n\n<style=cIsHealing>likes: snipin, bein badass (gearbox style), crowbars, the reaper (from deadbolt), killing, death, dubstep, backflips, razor chroma key, hot huntresses with big boobys who dress like sluts, decoys, the reaper (from real life), railguns, capes (the cool kind not the gay kind)</style>\n\n<style=cIsHealth>dislikes: spotter drones, wisps, frenzied elites, the enforcer from ues fuck you enforcer stop showin everyone my deviantart logs you peace of shit, mithrix, desk plants, space_cowboy226 (mega ass-faggot), rain, life, the captain, overloading worms</style>\n\n<color=#FF0000>@risk_of_rainin_blood</color>";
            R2API.LanguageAPI.Add("SNIPERCLASSIC_BODY_LORE", tldr);

            R2API.LanguageAPI.Add("KEYWORD_SNIPERCLASSIC_RELOADING", "<style=cKeywordName>Reloading</style><style=cSub>Using this skill instantly reloads your primary.</style>");
        }

        public void RegisterSurvivor()
        {
            SurvivorDef sniperDef = ScriptableObject.CreateInstance<SurvivorDef>();
            sniperDef.cachedName = "SniperClassic";
            sniperDef.bodyPrefab = SniperBody;
            sniperDef.descriptionToken = "SNIPERCLASSIC_DESCRIPTION";
            sniperDef.displayPrefab = SniperDisplay;
            sniperDef.primaryColor = SniperColor;
            sniperDef.outroFlavorToken = "SNIPERCLASSIC_OUTRO_FLAVOR";
            sniperDef.desiredSortPosition = 100f;
            SniperContent.survivorDefs.Add(sniperDef);
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
                    cb.bodyColor = SniperColor;

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

                    cb.portraitIcon = SniperContent.assetBundle.LoadAsset<Texture>("texSniperIcon3.png");
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

            R2API.LoadoutAPI.SkinDefInfo skinDefInfo = new R2API.LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>(),
                Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f / 255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), SniperColor),
                MeshReplacements = new SkinDef.MeshReplacement[0],
                /*{
                    new SkinDef.MeshReplacement
                    {
                        //renderer = mainRenderer,
                        //mesh = mainRenderer.sharedMesh
                    }
                };
                */
                Name = "SNIPERCLASSIC_DEFAULT_SKIN_NAME",
                NameToken = "SNIPERCLASSIC_DEFAULT_SKIN_NAME",
                RendererInfos = characterModel.baseRendererInfos,
                RootObject = model,
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0]
            };

            skinController.skins = new SkinDef[]
            {
                R2API.LoadoutAPI.CreateNewSkinDef(skinDefInfo)
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

            //SniperContent.entityStates.Add(typeof(BaseSnipeState));
            //SniperContent.entityStates.Add(typeof(BaseReloadState));

            SniperContent.entityStates.Add(typeof(AIReload));

            Sprite iconReload = SniperContent.assetBundle.LoadAsset<Sprite>("texPrimaryReloadIcon.png");

            SkillFamily primarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
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
            FireBattleRifle.reloadDef = primaryBRReloadDef;
            SniperContent.skillDefs.Add(primaryBRReloadDef);

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
            primaryHeavySnipeDef.keywordTokens = new string[] { };
            primaryHeavySnipeDef.mustKeyPress = false;
            primaryHeavySnipeDef.cancelSprintingOnActivation = true;
            primaryHeavySnipeDef.rechargeStock = 1;
            primaryHeavySnipeDef.requiredStock = 1;
            primaryHeavySnipeDef.skillName = "HeavySnipe";
            primaryHeavySnipeDef.skillNameToken = "SNIPERCLASSIC_PRIMARY_ALT2_NAME";
            primaryHeavySnipeDef.skillDescriptionToken = "SNIPERCLASSIC_PRIMARY_ALT2_DESCRIPTION";
            primaryHeavySnipeDef.stockToConsume = 1;
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

            SniperContent.skillFamilies.Add(primarySkillFamily);
        }

        public void AssignSecondary(SkillLocator sk)
        {
            ScopeCrosshairSetup();
            ScopeStateMachineSetup();

            SkillFamily secondarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            secondarySkillFamily.defaultVariantIndex = 0u;
            secondarySkillFamily.variants = new SkillFamily.Variant[1];
            sk.secondary._skillFamily = secondarySkillFamily;

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
            secondaryScopeDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSecondaryIcon.png");
            secondaryScopeDef.interruptPriority = InterruptPriority.Any;
            secondaryScopeDef.isCombatSkill = false;
            secondaryScopeDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            secondaryScopeDef.mustKeyPress = false;
            if (SecondaryScope.toggleScope || SecondaryScope.csgoZoom)
            {
                secondaryScopeDef.mustKeyPress = true;
            }
            secondaryScopeDef.cancelSprintingOnActivation = true;
            secondaryScopeDef.rechargeStock = 1;
            secondaryScopeDef.requiredStock = 0;
            secondaryScopeDef.skillName = "Scope";
            secondaryScopeDef.skillNameToken = "SNIPERCLASSIC_SECONDARY_NAME";
            secondaryScopeDef.skillDescriptionToken = "SNIPERCLASSIC_SECONDARY_DESCRIPTION";
            secondaryScopeDef.stockToConsume = 0;
            SniperContent.entityStates.Add(typeof(SecondaryScope));
            SniperContent.skillDefs.Add(secondaryScopeDef);
            SniperContent.skillFamilies.Add(secondarySkillFamily);
            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondaryScopeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondaryScopeDef.skillNameToken, false)
            };

            scopeDef = secondaryScopeDef;

            #region trickshot
            /*SkillDef trickshotDef = SkillDef.CreateInstance<SkillDef>();
            trickshotDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.Trickshot));
            trickshotDef.activationStateMachineName = "Weapon";
            trickshotDef.baseMaxStock = 1;
            trickshotDef.baseRechargeInterval = 6f;
            trickshotDef.beginSkillCooldownOnSkillEnd = false;
            trickshotDef.canceledFromSprinting = false;
            trickshotDef.dontAllowPastMaxStocks = true;
            trickshotDef.forceSprintDuringState = false;
            trickshotDef.fullRestockOnAssign = true;
            trickshotDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSecondaryIcon.png");
            trickshotDef.interruptPriority = InterruptPriority.Skill;
            trickshotDef.isCombatSkill = false;
            trickshotDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_RELOADING" };
            trickshotDef.mustKeyPress = false;
            trickshotDef.cancelSprintingOnActivation = false;
            trickshotDef.rechargeStock = 1;
            trickshotDef.requiredStock = 1;
            trickshotDef.skillName = "Trickshot";
            trickshotDef.skillNameToken = "SNIPERCLASSIC_SECONDARY_ALT_NAME";
            trickshotDef.skillDescriptionToken = "SNIPERCLASSIC_SECONDARY_ALT_DESCRIPTION";
            trickshotDef.stockToConsume = 1;
            SniperContent.entityStates.Add(typeof(Trickshot));
            SniperContent.skillDefs.Add(trickshotDef);
            Array.Resize(ref secondarySkillFamily.variants, secondarySkillFamily.variants.Length + 1);
            secondarySkillFamily.variants[secondarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = trickshotDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(trickshotDef.skillNameToken, false)
            };
            spinDef = trickshotDef;*/
            #endregion
        }
        public void ScopeCrosshairSetup()
        {
            SecondaryScope.scopeCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("ScopeCrosshair.prefab");
            SecondaryScope.scopeCrosshairPrefab.AddComponent<HudElement>();
            CrosshairController cc = SecondaryScope.scopeCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.scopeCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();

            SecondaryScope.noscopeCrosshairPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("NoscopeCrosshair.prefab");
            SecondaryScope.noscopeCrosshairPrefab.AddComponent<HudElement>();
            cc = SecondaryScope.noscopeCrosshairPrefab.AddComponent<CrosshairController>();
            cc.maxSpreadAngle = 2.5f;
            SecondaryScope.noscopeCrosshairPrefab.AddComponent<ScopeChargeIndicatorController>();

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
            sk.utility._skillFamily = utilitySkillFamily;

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
            utilityBackflipDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_RELOADING" };
            utilityBackflipDef.mustKeyPress = false;
            utilityBackflipDef.cancelSprintingOnActivation = false;
            utilityBackflipDef.rechargeStock = 1;
            utilityBackflipDef.requiredStock = 1;
            utilityBackflipDef.skillName = "CombatBackflip";
            utilityBackflipDef.skillNameToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_NAME";
            utilityBackflipDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_BACKFLIP_DESCRIPTION";
            utilityBackflipDef.stockToConsume = 1;
            SniperContent.entityStates.Add(typeof(Backflip));
            SniperContent.skillDefs.Add(utilityBackflipDef);
            SniperContent.skillFamilies.Add(utilitySkillFamily);
            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = utilityBackflipDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityBackflipDef.skillNameToken, false)
            };

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
            utilityRollDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texUtilityAltIcon.png");
            utilityRollDef.interruptPriority = InterruptPriority.Any;
            utilityRollDef.isCombatSkill = false;
            utilityRollDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_RELOADING" };
            utilityRollDef.mustKeyPress = false;
            utilityRollDef.cancelSprintingOnActivation = false;
            utilityRollDef.rechargeStock = 1;
            utilityRollDef.requiredStock = 1;
            utilityRollDef.skillName = "CombatRoll";
            utilityRollDef.skillNameToken = "SNIPERCLASSIC_UTILITY_NAME";
            utilityRollDef.skillDescriptionToken = "SNIPERCLASSIC_UTILITY_DESCRIPTION";
            utilityRollDef.stockToConsume = 1;
            SniperContent.entityStates.Add(typeof(CombatRoll));
            SniperContent.skillDefs.Add(utilityRollDef);
            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = utilityRollDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityRollDef.skillNameToken, false)
            };


            SkillDef utilitySmokeDef = SkillDef.CreateInstance<SkillDef>();
            utilitySmokeDef.activationState = new SerializableEntityStateType(typeof(AimSmokeGrenade));
            utilitySmokeDef.activationStateMachineName = "Scope";
            utilitySmokeDef.baseMaxStock = 1;
            utilitySmokeDef.baseRechargeInterval = 25f;
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
            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = utilitySmokeDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilitySmokeDef.skillNameToken, false)
            };
            SniperContent.skillDefs.Add(utilitySmokeDef);
            SniperContent.entityStates.Add(typeof(AimSmokeGrenade));
            SniperContent.entityStates.Add(typeof(FireSmokeGrenade));

            SniperContent.skillFamilies.Add(utilitySkillFamily);
        }

        public void AssignSpecial(SkillLocator sk)
        {
            DroneStateMachineSetup();
            SpotterFollowerSetup();

            SkillFamily specialSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            specialSkillFamily.defaultVariantIndex = 0u;
            specialSkillFamily.variants = new SkillFamily.Variant[1];
            sk.special._skillFamily = specialSkillFamily;

            SkillDef specialSpotDef = SkillDef.CreateInstance<SkillDef>();
            specialSpotDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.SendSpotter));
            specialSpotDef.activationStateMachineName = "DroneLauncher";
            specialSpotDef.baseMaxStock = 1;
            specialSpotDef.baseRechargeInterval = 7f;
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
            specialSpotDef.requiredStock = 1;
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
            specialSpotReturnDef.icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSpecialCancelIcon.png");
            specialSpotReturnDef.interruptPriority = InterruptPriority.Any;
            specialSpotReturnDef.isCombatSkill = false;
            specialSpotReturnDef.keywordTokens = new string[] { "KEYWORD_SNIPERCLASSIC_ANALYZED" };
            specialSpotReturnDef.mustKeyPress = true;
            specialSpotReturnDef.cancelSprintingOnActivation = false;
            specialSpotReturnDef.rechargeStock = 1;
            specialSpotReturnDef.requiredStock = 1;
            specialSpotReturnDef.skillName = "ReturnSpot";
            specialSpotReturnDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_NAME";
            specialSpotReturnDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_DESCRIPTION";
            specialSpotReturnDef.stockToConsume = 1;

            spotReturnDef = specialSpotReturnDef;

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
            specialSpotScepterDef.baseRechargeInterval = 7f;
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
            specialSpotScepterDef.requiredStock = 1;
            specialSpotScepterDef.skillName = "Spot";
            specialSpotScepterDef.skillNameToken = "SNIPERCLASSIC_SPECIAL_SCEPTER_NAME";
            specialSpotScepterDef.skillDescriptionToken = "SNIPERCLASSIC_SPECIAL_SCEPTER_DESCRIPTION";
            specialSpotScepterDef.stockToConsume = 1;

            spotScepterDef = specialSpotScepterDef;
            SniperContent.entityStates.Add(typeof(SendSpotterScepter));
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
            GameObject spotterObject = SniperContent.assetBundle.LoadAsset<GameObject>("mdlSpotter.prefab");
            spotterObject.AddComponent<SpotterFollowerController>();
            ClientScene.RegisterPrefab(spotterObject);
            SpotterTargetingController.spotterFollowerGameObject = spotterObject;
            spotterObject.GetComponentInChildren<MeshRenderer>().material = Modules.Skins.CreateMaterial("matSniper", 3f, new Color(1f, 163f / 255f, 92f / 255f));
        }

        public void ReadConfig()
        {
            arenaNerf = base.Config.Bind<bool>(new ConfigDefinition("00 - General", "Kings Kombat Arena Nerf"), true, new ConfigDescription("Disable Spotter Slow when Kings Kombat Arena is active.")).Value;

            ConfigEntry<bool> scopeCSGOZoom = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Preset Zoom (Overrides all other settings)"), false, new ConfigDescription("Pressing M2 cycles through preset zoom levels."));
            ConfigEntry<bool> scopeToggle = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Toggle Scope"), false, new ConfigDescription("Makes Steady Aim not require you to hold down the skill key to use."));
            ConfigEntry<float> scopeZoomFOV = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Default FOV"), 50f, new ConfigDescription("Default zoom level of Steady Aim (accepts values from 5-50)."));
            ConfigEntry<bool> scopeResetZoom = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Reset Zoom on Unscope"), false, new ConfigDescription("Reset scope zoom level when unscoping."));
            ConfigEntry<bool> scopeUseScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Use Scroll Wheel for Zoom"), true, new ConfigDescription("Scroll wheel changes zoom level. Scroll up to zoom in, scroll down to zoom out."));
            ConfigEntry<bool> scopeInvertScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("20 - Secondary - Steady Aim", "Invert Scroll Wheel"), false, new ConfigDescription("Reverses scroll wheel direction. Scroll up to zoom out, scroll down to zoom in."));
            ConfigEntry<float> scopeScrollZoomSpeed = base.Config.Bind<float>(new ConfigDefinition("20 - Secondary - Steady Aim", "Scroll Wheel Zoom Speed"), 30f, new ConfigDescription("Zoom speed when using the scroll wheel."));
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
                SniperContent.assetBundle = AssetBundle.LoadFromStream(stream);
            }

            ReloadController.reloadBar = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadBar.png");
            ReloadController.reloadCursor = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadSlider.png");
            ReloadController.reloadBarFail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadBarFail.png");
            ReloadController.reloadCursorFail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadSliderFail.png");
            ReloadController.indicatorGood = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadGood.png");
            ReloadController.indicatorPerfect = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadPerfect.png");
            ScopeController.stockEmpty = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadEmptypng");
            ScopeController.stockAvailable = ReloadController.indicatorGood;

            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SniperClassic.SniperClassic_Sounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                R2API.SoundAPI.SoundBanks.Add(bytes);
            }
        }

        private void CreateMaster()
        {
            GameObject SniperMonsterMaster = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "SniperClassicMonsterMaster", true);
            SniperContent.masterPrefabs.Add(SniperMonsterMaster);

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
            roll.maxDistance = 30f;
            roll.selectionRequiresTargetLoS = false;
            roll.activationRequiresTargetLoS = false;
            roll.activationRequiresAimConfirmation = false;
            roll.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            roll.aimType = AISkillDriver.AimType.MoveDirection;
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
            scopeAggressive.minDistance = 30f;
            scopeAggressive.maxDistance = float.PositiveInfinity;
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
            scopeAggressive.requiredSkill = scopeDef;

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
            strafeShoot.noRepeat = false;
            strafeShoot.shouldSprint = false;
            strafeShoot.shouldFireEquipment = false;
            strafeShoot.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            strafeShoot.driverUpdateTimerOverride = 1f;

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

        private void SetupNeedleRifleProjectile()
        {
            GameObject needleProjectile = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/lunarneedleprojectile"), "SniperClassicNeedleRifleProjectile", true);
            SniperContent.projectilePrefabs.Add(needleProjectile);

            ProjectileImpactExplosion pie = needleProjectile.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = 4f;
            NeedleRifle.projectilePrefab = needleProjectile;
            SniperContent.entityStates.Add(typeof(NeedleRifle));
        }

        //based on https://github.com/GnomeModder/EnforcerMod/blob/master/EnforcerMod_VS/EnforcerPlugin.cs
        private void SetupSmokeGrenade()

        {
            GameObject smokeProjectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile").InstantiateClone("SniperClassic_SmokeGrenade", true);
            GameObject smokePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone").InstantiateClone("SniperClassic_SmokeDotZone", true);

            ProjectileController grenadeController = smokeProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileController tearGasController = smokePrefab.GetComponent<ProjectileController>();

            ProjectileDamage grenadeDamage = smokeProjectilePrefab.GetComponent<ProjectileDamage>();
            ProjectileDamage smokeDamage = smokePrefab.GetComponent<ProjectileDamage>();

            ProjectileSimple simple = smokeProjectilePrefab.GetComponent<ProjectileSimple>();

            TeamFilter filter = smokePrefab.GetComponent<TeamFilter>();

            ProjectileImpactExplosion grenadeImpact = smokeProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            Destroy(smokePrefab.GetComponent<ProjectileDotZone>());

            BuffWard buffWard = smokePrefab.AddComponent<BuffWard>();
            BuffWard debuffWard = smokePrefab.AddComponent<BuffWard>();

            filter.teamIndex = TeamIndex.Player;

            GameObject grenadeModel = SniperContent.assetBundle.LoadAsset<GameObject>("SmokeGrenade").InstantiateClone("SniperClassic_SmokeGhost", true);
            grenadeModel.AddComponent<NetworkIdentity>();
            grenadeModel.AddComponent<ProjectileGhostController>();

            grenadeController.ghostPrefab = grenadeModel;
            //tearGasController.ghostPrefab = Assets.tearGasEffectPrefab;

            grenadeImpact.offsetForLifetimeExpiredSound = 1;
            grenadeImpact.destroyOnEnemy = false;
            grenadeImpact.destroyOnWorld = true;
            grenadeImpact.timerAfterImpact = false;
            grenadeImpact.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            grenadeImpact.lifetime = 12f;
            grenadeImpact.lifetimeRandomOffset = 0;
            grenadeImpact.blastRadius = 18f;
            grenadeImpact.blastDamageCoefficient = 1;
            grenadeImpact.blastProcCoefficient = 1;
            grenadeImpact.fireChildren = true;
            grenadeImpact.childrenCount = 1;
            grenadeImpact.childrenProjectilePrefab = smokePrefab;
            grenadeImpact.childrenDamageCoefficient = 0;
            grenadeImpact.impactEffect = null;

            grenadeController.startSound = "";
            grenadeController.procCoefficient = 1;
            tearGasController.procCoefficient = 0;

            grenadeDamage.crit = false;
            grenadeDamage.damage = 0f;
            grenadeDamage.damageColorIndex = DamageColorIndex.Default;
            grenadeDamage.damageType = DamageType.Stun1s | DamageType.NonLethal;
            grenadeDamage.force = 0;

            smokeDamage.crit = false;
            smokeDamage.damage = 0;
            smokeDamage.damageColorIndex = DamageColorIndex.WeakPoint;
            smokeDamage.damageType = DamageType.Stun1s | DamageType.NonLethal;
            smokeDamage.force = 0;

            buffWard.radius = 12;
            buffWard.interval = 0.5f;
            buffWard.rangeIndicator = null;
            buffWard.buffDef = RoR2Content.Buffs.Cloak;
            buffWard.buffDuration = 1f;
            buffWard.floorWard = false;
            buffWard.expires = false;
            buffWard.invertTeamFilter = false;
            buffWard.expireDuration = 0;
            buffWard.animateRadius = false;

            debuffWard.radius = 12;
            debuffWard.interval = 0.5f;
            debuffWard.rangeIndicator = null;
            debuffWard.buffDef = RoR2Content.Buffs.Slow50;
            debuffWard.buffDuration = 1f;
            debuffWard.floorWard = false;
            debuffWard.expires = false;
            debuffWard.invertTeamFilter = true;
            debuffWard.expireDuration = 0;
            debuffWard.animateRadius = false;

            Destroy(smokePrefab.transform.GetChild(0).gameObject);
            GameObject gasFX = SniperContent.assetBundle.LoadAsset<GameObject>("SmokeEffect").InstantiateClone("FX", false);
            gasFX.AddComponent<DestroyOnTimer>().duration = 12f;
            gasFX.transform.parent = smokePrefab.transform;
            gasFX.transform.localPosition = Vector3.zero;

            smokePrefab.AddComponent<DestroyOnTimer>().duration = 12f;

            SniperContent.projectilePrefabs.Add(smokeProjectilePrefab);
            SniperContent.projectilePrefabs.Add(smokePrefab);
            FireSmokeGrenade.projectilePrefab = smokeProjectilePrefab;

            smokePrefab.AddComponent<SmokeSound>();
        }

        private void SetupHeavySnipeProjectile()
        {
            GameObject hsProjectile = Resources.Load<GameObject>("prefabs/projectiles/fireball").InstantiateClone("MoffeinSniperClassicHeavyBullet",true);
            hsProjectile.transform.localScale *= 0.5f;
            hsProjectile.AddComponent<DamageOverDistance>();
            Rigidbody rb = hsProjectile.GetComponent<Rigidbody>();
            rb.useGravity = true;

            ProjectileController pc = hsProjectile.GetComponent<ProjectileController>();

            ProjectileSimple ps = hsProjectile.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 240;
            ps.lifetime = 10f;

            Destroy(hsProjectile.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileImpactExplosion pie = hsProjectile.AddComponent<ProjectileImpactExplosion>();
            pie.destroyOnEnemy = true;
            pie.destroyOnWorld = true;
            pie.blastDamageCoefficient = 1f;
            pie.blastProcCoefficient = 1f;
            pie.blastAttackerFiltering = AttackerFiltering.Default;
            pie.blastRadius = 5f;
            pie.lifetime = 60f;
            pie.falloffModel = BlastAttack.FalloffModel.None;
            pie.explosionEffect = BuildHeavySnipeExplosionEffect();

            GameObject hsProjectileGhost = Resources.Load<GameObject>("prefabs/projectileghosts/FireballGhost").InstantiateClone("MoffeinSniperClassicHeavyBulletGhost", false);
            hsProjectileGhost.transform.localScale *= 0.5f;
            pc.ghostPrefab = hsProjectileGhost;

            SniperContent.projectilePrefabs.Add(hsProjectile);
            HeavySnipe.projectilePrefab = hsProjectile;
        }

        private GameObject BuildHeavySnipeExplosionEffect()
        {
            GameObject effect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX").InstantiateClone("MoffeinSniperClassicExplosionEffect", false);
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_MULT_m1_grenade_launcher_explo";
            ec.applyScale = true;

            SniperContent.effectDefs.Add(new EffectDef(effect));
            return effect;
        }
    }
}