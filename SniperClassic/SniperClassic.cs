using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.SniperClassicSkills;
using JetBrains.Annotations;
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
using SniperClassic.Hooks;
using SniperClassic.Modules;
using SniperClassic.Modules.Achievements;
using SniperClassic.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.UnlockableAPI.PluginGUID)]
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]

    [BepInDependency("com.Kingpinush.KingKombatArena", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.ClassicItems", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("HIFU.Inferno", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin("com.Moffein.SniperClassic", "Sniper Classic", "1.5.16")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    public class SniperClassic : BaseUnityPlugin
    {

        //weight paint coat
        readonly Shader hotpoo = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/hgstandard");
        public static GameObject SniperBody = null;
        public static GameObject SniperDisplay = null;
        public static Color SniperColor = new Color(78f / 255f, 80f / 255f, 111f / 255f);

        public static bool scepterInstalled = false;
        public static bool classicItemsInstalled = false;

        public static bool arenaNerf = true;
        public static bool arenaPluginLoaded = false;
        public static bool arenaActive = false;

        public static bool riskOfOptionsLoaded = false;

        public static bool emotesLoaded = false;

        public static bool starstormInstalled = false;

        public static bool changeSortOrder = false;

        public static bool infernoPluginLoaded = false;

        public static bool enableWeakPoints = true;

        public void Awake()
        {
            Files.PluginInfo = Info;
            CompatSetup();
            Setup();
            SoundBanks.Init();
            Nemesis.Setup();
            AddHooks();
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            ContentManager.onContentPacksAssigned += ContentManager_onContentPacksAssigned;
        }

        private void CompatSetup()
        {
            riskOfOptionsLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
            scepterInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
            classicItemsInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems");
            arenaPluginLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Kingpinush.KingKombatArena");
            starstormInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2");
            emotesLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
            infernoPluginLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("HIFU.Inferno");

            if (emotesLoaded) EmoteAPICompat();
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new SniperContent());
        }
        
        private void ContentManager_onContentPacksAssigned(HG.ReadOnlyArray<ReadOnlyContentPack> obj) {
            Modules.ItemDisplays.RegisterDisplays();
        }

        public void Setup()
        {
            SniperContent.SpotterDebuffOnHit = DamageAPI.ReserveDamageType();
            SniperContent.FullCharge = DamageAPI.ReserveDamageType();

            LoadResources();
            Modules.Config.ReadConfig(base.Config);
            Modules.Assets.InitializeAssets();

            Modules.ItemDisplays.Initialize();
            CreatePrefab();
            CreateDisplayPrefab();
            SetupStats();
            BuildEffects.Init();
            Modules.Achievements.SniperUnlockables.RegisterUnlockables();
            Modules.SniperSkins.RegisterSkins();
            BuildSkills.Init();
            RegisterSurvivor();
            Modules.Tokens.RegisterLanguageTokens();
            BuildMaster.Init();
            BuildProjectiles.Init();
        }

        private void AddHooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats.RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GlobalEventManager.ProcessHitEnemy += OnHitEnemy.GlobalEventManager_OnHitEnemy;
            new DetectArenaMode();
            new ScopeNeedleRifle();
            new AIDrawAggro();
            new StealBuffVisuals();
            new FixReloadMenuUI();
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
            GameObject characterPrefab = R2API.PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "SniperClassicBody", true);

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
            gameObject3.transform.localPosition = new Vector3(0f, 2.4f, 0f);
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

            CharacterCameraParams cc = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraTargetParams.cameraParams = cc;

            cc.data.maxPitch = 70;
            cc.data.minPitch = -70;
            cc.data.wallCushion = 0.1f;
            cc.data.pivotVerticalOffset = 0.5f;
            cc.data.idealLocalCameraPos = Vector3.zero;

            //cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cc.data.idealLocalCameraPos = new Vector3(0, -0.3f, -8.2f); //used to be standardLocalCameraPos

            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = characterPrefab.GetComponent<CharacterBody>();

            characterModel.baseRendererInfos = SetRendererInfosFromModel(childLocator);
            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - 1].renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlayInstance>();

            TeamComponent teamComponent;
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

            EntityStateMachine stateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(SniperMain));
            SniperContent.entityStates.Add(typeof(SniperMain));

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = stateMachine;
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

            //kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            //kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeAndDenivelationHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.playerCharacter = true;
            //kinematicCharacterMotor.SafeMovement = false;

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
            mainHurtbox.isSniperTarget = true;

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
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");

            RagdollController ragdollController = model.GetComponent<RagdollController>();
            PhysicMaterial physicMat = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;
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

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.baseRendererInfos = SniperBody.GetComponentInChildren<CharacterModel>().baseRendererInfos;

            SniperDisplay = R2API.PrefabAPI.InstantiateClone(model, "SniperClassicDisplay", false);

            SniperDisplay.AddComponent<MenuSoundComponent>();
            SniperDisplay.GetComponent<CharacterSelectSurvivorPreviewDisplayController>().bodyPrefab = SniperBody;
        }

        //after almost two years finally this code isn't duplicated in two places
        private static CharacterModel.RendererInfo[] SetRendererInfosFromModel(ChildLocator childLocator)
        {
            Material sniperMat = Modules.Assets.CreateMaterial("matSniperDefault", 1.2f, Color.white);
            Material sniperGunMat = Modules.Assets.CreateMaterial("matSniperDefault", 5f, new Color(192f / 255f, 152f / 255f, 216f / 255f));
            Material spotterMat = Modules.Assets.CreateMaterial("matSniperDefault", 3f, new Color(1f, 163f / 255f, 92f / 255f));

            CharacterModel.RendererInfo[] rendererInfos = new CharacterModel.RendererInfo[]
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
                    renderer = childLocator.FindChild("GunAltModel").GetComponent<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = spotterMat,
                    renderer = childLocator.FindChild("SpotterModel").GetComponent<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = sniperMat,
                    renderer = childLocator.FindChild("BeretModel").GetComponent<SkinnedMeshRenderer>(),
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
            return rendererInfos;
        }

        public void RegisterSurvivor()
        {
            SurvivorDef sniperDef = ScriptableObject.CreateInstance<SurvivorDef>();              
            sniperDef.cachedName = "SniperClassic";                                              
            sniperDef.bodyPrefab = SniperBody;
            sniperDef.displayNameToken = "SNIPERCLASSIC_BODY_NAME";
            sniperDef.descriptionToken = "SNIPERCLASSIC_DESCRIPTION";                            
            sniperDef.displayPrefab = SniperDisplay;                                             
            sniperDef.primaryColor = SniperColor;                                                
            sniperDef.outroFlavorToken = "SNIPERCLASSIC_OUTRO_FLAVOR";                           
            sniperDef.desiredSortPosition = SniperClassic.changeSortOrder ? 7.5f : 67f;          
            sniperDef.unlockableDef = SniperUnlockables.CharacterUnlockableDef;                  
            SniperContent.survivorDefs.Add(sniperDef);                                           
        }                                                                                        


        public void SetupStats()
        {
            if (SniperBody)
            {
                SniperBody.AddComponent<ScopeController>();
                SniperBody.AddComponent<ReloadController>();
                SniperBody.AddComponent<SpotterTargetingController>();
                SniperBody.AddComponent<SpotterRechargeController>();
                CharacterBody cb = SniperBody.GetComponent<CharacterBody>();
                if (cb)
                {
                    cb.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
                    cb.baseNameToken = "SNIPERCLASSIC_BODY_NAME";
                    cb.subtitleNameToken = "SNIPERCLASSIC_BODY_SUBTITLE";
                    cb._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/crosshair/StandardCrosshair");
                    cb.baseMaxHealth = 110f;
                    cb.baseRegen = 1f;
                    cb.baseMaxShield = 0f;
                    cb.baseMoveSpeed = 7f;
                    cb.baseAcceleration = 80f;
                    cb.baseJumpPower = 15f;
                    cb.baseDamage = 14f;
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

                    //Spread curve. Copied from Bandit2
                    Keyframe key1 = new Keyframe(0f, 0f, 1.505722f, 1.505722f, 0f, 0.05012326f);
                    key1.weightedMode = WeightedMode.None;

                    Keyframe key2 = new Keyframe(1f, 8f, 2.360606f, 2.360606f, 0.08792114f, 0f);
                    key2.weightedMode = WeightedMode.None;

                    AnimationCurve spreadCurve = new AnimationCurve(new Keyframe[] { key1, key2 });
                    cb.spreadBloomCurve = spreadCurve;

                    cb.spreadBloomDecayTime = 0.7f; //bandit is 0.5
                }
            }
        }

        public void LoadResources()
        {
            if (!SniperContent.assetBundle) SniperContent.assetBundle = AssetBundle.LoadFromFile(Files.GetPathToFile("AssetBundles", "sniperclassic"));

            //This could be REALLY simplified if it just used Unity's Image Color field instead.
            ReloadController.indicatorGood = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadGood.png");
            ReloadController.indicatorPerfect = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadPerfect.png");
            ReloadController.indicatorMarkStock = SniperContent.assetBundle.LoadAsset<Texture2D>("texCrosshairDotCropped.png");

            ReloadController.reloadBar2Border = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2Border.png");
            ReloadController.reloadBar2BorderFinish = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2BorderFinish.png");
            ReloadController.reloadBar2 = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2Bar.png");
            ReloadController.reloadBar2Fail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2BorderBad.png");
            ReloadController.reloadBar2Good = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2BorderGood.png");
            ReloadController.reloadBar2Perfect = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2BorderPerfect.png");
            ReloadController.reloadSlider2 = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2Slider.png");
            ReloadController.reloadSlider2Fail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2SliderFail.png");
            ReloadController.reloadRegionGood = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2RegionGood.png");
            ReloadController.reloadRegionPerfect = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2RegionPerfect.png");
            ReloadController.reloadRegionGoodFail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2RegionGoodBad.png");
            ReloadController.reloadRegionPerfectFail = SniperContent.assetBundle.LoadAsset<Texture2D>("texReload2RegionPerfectBad.png");

            ScopeController.stockEmpty = SniperContent.assetBundle.LoadAsset<Texture2D>("texReloadEmpty.png");
            ScopeController.stockAvailable = ReloadController.indicatorGood;
            ScopeController.stockAvailable = ReloadController.indicatorGood;

            var highlightPrefab = SniperContent.assetBundle.LoadAsset<GameObject>("SpotterTargetHighlight.prefab");
            var highlightComponent = highlightPrefab.AddComponent<SpotterFollowerController.SpotterTargetHighlight>();
            highlightComponent.insideViewObject = highlightPrefab.transform.Find("Pivot").gameObject;
            highlightComponent.outsideViewObject = highlightPrefab.transform.Find("PivotOutsideView").gameObject;

            if (!Modules.Config.spotterUI)
            {
                Transform rectHUD = highlightPrefab.transform.Find("Pivot/Rectangle");
                rectHUD.localScale = Vector3.zero;
            }

            highlightComponent.textTargetName = highlightPrefab.transform.Find("Pivot/Rectangle/Enemy Name").gameObject.GetComponent<TextMeshProUGUI>();
            highlightComponent.textTargetHP = highlightPrefab.transform.Find("Pivot/Rectangle/Health").gameObject.GetComponent<TextMeshProUGUI>();
            SpotterFollowerController.spotterTargetHighlightPrefab = highlightPrefab;
            RoR2Application.onLateUpdate += SpotterFollowerController.SpotterTargetHighlight.UpdateAll;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void EmoteAPICompat()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                foreach (var item in SurvivorCatalog.allSurvivorDefs)
                {
                    if (item.bodyPrefab.name == "SniperClassicBody")
                    {
                        var skele = SniperContent.assetBundle.LoadAsset<GameObject>("animSniperClassic.prefab");
                        EmotesAPI.CustomEmotesAPI.ImportArmature(item.bodyPrefab, skele);
                        skele.GetComponentInChildren<BoneMapper>().scale = 1.5f;
                    }
                }
            };
        }
    }
}
