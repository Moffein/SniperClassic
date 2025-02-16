using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using SniperClassic.Modules;
using UnityEngine;
using static R2API.DamageAPI;

namespace EntityStates.SniperClassicSkills
{
    class HeavySnipe : BaseSnipeState
    {
        public override void SetStats()
        {
            internalDamage = damageCoefficient;
            internalRadius = radius;
            internalForce = force;
            internalBaseDuration = baseDuration;
            internalAttackSoundString = attackSoundString;
            internalChargedAttackSoundString = chargedAttackSoundString;
            internalRecoilAmplitude = recoilAmplitude;
            internalReloadDef = reloadDef;
            internalReloadBarLength = reloadBarLength;
        }

        public override void FireBullet(Ray aimRay, float chargeMult, bool crit)
        {
            bool fullCharge = chargeMult >= SniperClassic.ScopeController.baseMaxChargeMult;
            FireProjectileInfo fpi = new FireProjectileInfo
            {
                projectilePrefab = fullCharge ? headshotProjectilePrefab : projectilePrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = base.gameObject,
                damage = this.damageStat * damageCoefficient * chargeMult * reloadDamageMult,
                force = force,
                crit = crit,
                damageColorIndex = DamageColorIndex.Default,
                target = null,
                speedOverride = -1f,
                fuseOverride = -1f,
                damageTypeOverride = null
            };

            DamageTypeCombo desiredDamageType = (DamageTypeCombo)(DamageType.Generic) | (charge > 0f ? DamageSource.Secondary : DamageSource.Primary);
            if (fullCharge)
            {
                desiredDamageType.AddModdedDamageType(SniperContent.FullCharge);
                if (!SniperClassic.SniperClassic.arenaActive && SniperClassic.SniperClassic.arenaNerf) desiredDamageType.damageType |= DamageType.Stun1s;
            }
            fpi.damageTypeOverride = desiredDamageType;

            ProjectileManager.instance.FireProjectile(fpi);
        }

        public static GameObject projectilePrefab;
        public static GameObject headshotProjectilePrefab;
        public static float damageCoefficient = 5.4f;
        public static float radius = 0.4f;
        public static float force = 2500f;
        public static float baseDuration = 0.4f;
        public static float baseChargeDuration = 3f;
        public static float reloadBarLength = 1f;
        public static SkillDef reloadDef;
        public static string attackSoundString = "Play_SniperClassic_m1_heavy";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_shoot";
        public static float recoilAmplitude = 2.5f;
    }
}
