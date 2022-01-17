using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

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
            ProjectileManager.instance.FireProjectile(
                projectilePrefab,
                aimRay.origin,
                Util.QuaternionSafeLookRotation(aimRay.direction),
                base.gameObject,
                this.damageStat * damageCoefficient * chargeMult * reloadDamageMult,
                force,
                crit,
                DamageColorIndex.Default,
                null,
                -1f);
        }

        public static GameObject projectilePrefab;
        public static float damageCoefficient = 5.4f;
        public static float radius = 0.4f;
        public static float force = 2500f;
        public static float baseDuration = 0.4f;
        public static float baseChargeDuration = 4f;
        public static float reloadBarLength = 0.6f;
        public static SkillDef reloadDef;
        public static string attackSoundString = "Play_SniperClassic_m1_heavy";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_shoot";
        public static float recoilAmplitude = 2.5f;
    }
}
