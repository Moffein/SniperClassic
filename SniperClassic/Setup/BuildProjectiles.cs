using SniperClassic.Controllers;
using RoR2;
using UnityEngine;
using SniperClassic.Hooks;
using RoR2.Projectile;
using SniperClassic.Modules;
using EntityStates.SniperClassicSkills;
using UnityEngine.Networking;
using R2API;

namespace SniperClassic.Setup
{
    public static class BuildProjectiles
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            SetupNeedleRifleProjectile();
            SetupSmokeGrenade();
            SetupHeavySnipeProjectile();
        }

        private static void SetupNeedleRifleProjectile()
        {
            GameObject needleProjectile = R2API.PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/lunarneedleprojectile"), "SniperClassicNeedleRifleProjectile", true);
            SniperContent.projectilePrefabs.Add(needleProjectile);

            ProjectileImpactExplosion pie = needleProjectile.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = 4f;
            UnityEngine.Object.Destroy(needleProjectile.GetComponent<ProjectileDirectionalTargetFinder>());
            UnityEngine.Object.Destroy(needleProjectile.GetComponent<ProjectileSteerTowardTarget>());
            UnityEngine.Object.Destroy(needleProjectile.GetComponent<ProjectileTargetComponent>());
            ScopeNeedleRifle.projectilePrefab = needleProjectile;


            ScopeNeedleRifle.headshotProjectilePrefab = R2API.PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/lunarneedleprojectile"), "SniperClassicNeedleRifleProjectileHeadshot", true);
            if (SniperClassic.enableWeakPoints)
            {
                ScopeNeedleRifle.headshotProjectilePrefab.AddComponent<Components.ProjectileHeadshotComponent>();
            }
            ProjectileImpactExplosion pie2 = ScopeNeedleRifle.headshotProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = 4f;
            UnityEngine.Object.Destroy(ScopeNeedleRifle.headshotProjectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>());
            UnityEngine.Object.Destroy(ScopeNeedleRifle.headshotProjectilePrefab.GetComponent<ProjectileSteerTowardTarget>());
            UnityEngine.Object.Destroy(ScopeNeedleRifle.headshotProjectilePrefab.GetComponent<ProjectileTargetComponent>());

            DamageAPI.ModdedDamageTypeHolderComponent mdh = ScopeNeedleRifle.headshotProjectilePrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            mdh.Add(SniperContent.FullCharge);
            SniperContent.projectilePrefabs.Add(ScopeNeedleRifle.headshotProjectilePrefab);
        }

        private static void SetupHeavySnipeProjectile()
        {
            GameObject hsProjectileGhost = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/FireballGhost").InstantiateClone("MoffeinSniperClassicHeavyBulletGhost", false);
            hsProjectileGhost.transform.localScale *= 0.25f;

            GameObject mortarProjectile = BuildHeavySnipeProjectileInternal("MoffeinSniperClassicHeavyBullet", hsProjectileGhost, false);
            SniperContent.projectilePrefabs.Add(mortarProjectile);
            HeavySnipe.projectilePrefab = mortarProjectile;

            GameObject mortarHeadshotProjectile = BuildHeavySnipeProjectileInternal("MoffeinSniperClassicHeavyBulletHeadshot", hsProjectileGhost, true);
            DamageAPI.ModdedDamageTypeHolderComponent mdh = mortarHeadshotProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            mdh.Add(SniperContent.FullCharge);
            SniperContent.projectilePrefabs.Add(mortarHeadshotProjectile);
            HeavySnipe.headshotProjectilePrefab = mortarHeadshotProjectile;
        }

        private static GameObject BuildHeavySnipeProjectileInternal(string projectileName, GameObject ghostPrefab, bool canHeadshot)
        {
            GameObject hsProjectile = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/fireball").InstantiateClone(projectileName, true);
            hsProjectile.transform.localScale *= 0.5f;


            if (canHeadshot)
            {
                if (SniperClassic.enableWeakPoints)
                {
                    hsProjectile.AddComponent<Components.ProjectileHeadshotComponent>();
                }
            }
            hsProjectile.AddComponent<DamageOverDistance>();
            Rigidbody rb = hsProjectile.GetComponent<Rigidbody>();
            rb.useGravity = true;

            ProjectileController pc = hsProjectile.GetComponent<ProjectileController>();
            pc.ghostPrefab = ghostPrefab;

            ProjectileSimple ps = hsProjectile.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 240;
            ps.lifetime = 10f;

            UnityEngine.Object.Destroy(hsProjectile.GetComponent<ProjectileSingleTargetImpact>());

            AntiGravityForce agf = hsProjectile.AddComponent<AntiGravityForce>();
            agf.antiGravityCoefficient = 0.75f;
            agf.rb = rb;

            ProjectileImpactExplosion pie = hsProjectile.AddComponent<ProjectileImpactExplosion>();
            pie.destroyOnEnemy = true;
            pie.destroyOnWorld = true;
            pie.blastDamageCoefficient = 1f;
            pie.blastProcCoefficient = 1f;
            pie.blastAttackerFiltering = AttackerFiltering.Default;
            pie.blastRadius = 5f;
            pie.lifetime = 60f;
            pie.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            pie.explosionEffect = BuildHeavySnipeExplosionEffect();
            pie.timerAfterImpact = true;
            pie.lifetimeAfterImpact = 0f;

            return hsProjectile;
        }

        private static GameObject BuildHeavySnipeExplosionEffect()
        {
            GameObject effect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX").InstantiateClone("MoffeinSniperClassicExplosionEffect", false);
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_SniperClassic_heavysnipe_explode";
            ec.applyScale = true;

            SniperContent.effectDefs.Add(new EffectDef(effect));
            return effect;
        }

        //based on https://github.com/GnomeModder/EnforcerMod/blob/master/EnforcerMod_VS/EnforcerPlugin.cs
        private static void SetupSmokeGrenade()

        {
            GameObject smokeProjectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile").InstantiateClone("SniperClassic_SmokeGrenade", true);
            GameObject smokePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone").InstantiateClone("SniperClassic_SmokeDotZone", true);

            ProjectileController grenadeController = smokeProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileController tearGasController = smokePrefab.GetComponent<ProjectileController>();

            ProjectileDamage grenadeDamage = smokeProjectilePrefab.GetComponent<ProjectileDamage>();
            ProjectileDamage smokeDamage = smokePrefab.GetComponent<ProjectileDamage>();

            ProjectileSimple simple = smokeProjectilePrefab.GetComponent<ProjectileSimple>();

            TeamFilter filter = smokePrefab.GetComponent<TeamFilter>();

            ProjectileImpactExplosion grenadeImpact = smokeProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            UnityEngine.Object.Destroy(smokePrefab.GetComponent<ProjectileDotZone>());

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
            buffWard.buffDef = RoR2Content.Buffs.Cloak;
            buffWard.buffDuration = 1f;
            buffWard.floorWard = false;
            buffWard.expires = false;
            buffWard.invertTeamFilter = false;
            buffWard.expireDuration = 0;
            buffWard.animateRadius = false;
            buffWard.rangeIndicator = null;

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

            float smokeDuration = 7f;

            UnityEngine.Object.Destroy(smokePrefab.transform.GetChild(0).gameObject);
            GameObject gasFX = SniperContent.assetBundle.LoadAsset<GameObject>("SmokeEffect").InstantiateClone("FX", false);
            gasFX.AddComponent<DestroyOnTimer>().duration = smokeDuration;
            gasFX.transform.parent = smokePrefab.transform;
            gasFX.transform.localPosition = Vector3.zero;

            smokePrefab.AddComponent<DestroyOnTimer>().duration = smokeDuration;

            SniperContent.projectilePrefabs.Add(smokeProjectilePrefab);
            SniperContent.projectilePrefabs.Add(smokePrefab);
            FireSmokeGrenade.projectilePrefab = smokeProjectilePrefab;
        }
    }
}
