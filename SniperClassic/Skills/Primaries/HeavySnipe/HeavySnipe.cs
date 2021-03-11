using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Skills;

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

        public static float damageCoefficient = 4.8f;
        public static float radius = 0.4f;
        public static float force = 750f;
        public static float baseDuration = 0.4f;
        public static float baseChargeDuration = 4.5f;
        public static float reloadBarLength = 1.4f;
        public static SkillDef reloadDef;
        public static string attackSoundString = "Play_SniperClassic_m1_heavy";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_shoot";
        public static float recoilAmplitude = 2.5f;
    }
}
