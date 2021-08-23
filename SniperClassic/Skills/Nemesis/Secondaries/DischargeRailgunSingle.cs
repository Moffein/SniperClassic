using SniperClassic.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace EntityStates.SniperClassicSkills.Nemesis
{
    public class DischargeRailgunSingle : BaseState
    {
        public static float minDamageCoefficient = 2.6f;
        public static float maxDamageCoefficient = 14f;
        public static float minForce = 1000f;
        public static float maxForce = 3000f;
        public static float minBulletRadius = 0.4f;
        public static float maxBulletRadius = 1.2f;
        public static float baseDuration = 0.83f;

        public static GameObject tracerEffectPrefab;
        public static GameObject hitEffectPrefab;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = DischargeRailgunSingle.baseDuration;

            RailgunHeatController rhc = base.GetComponent<RailgunHeatController>();
            if (rhc)
            {
                if (!rhc.empty)
                {
                    //Todo: add animation of railgun opening up spectacularly
                    //Todo: add custom big tracer for charged shots.
                    Util.PlaySound("Play_SniperClassicNemesis_RailgunDischarge", base.gameObject);
                    if (base.isAuthority)
                    {
                        float charge = rhc.DischargeRailgunSingle(duration);
                        Ray aimRay = base.GetAimRay();
                        new BulletAttack
                        {
                            owner = base.gameObject,
                            weapon = base.gameObject,
                            origin = aimRay.origin,
                            aimVector = aimRay.direction,
                            minSpread = 0f,
                            maxSpread = 0f,
                            bulletCount = 1u,
                            procCoefficient = 1f,
                            damage = Mathf.Lerp(DischargeRailgunSingle.minDamageCoefficient, DischargeRailgunSingle.maxDamageCoefficient, charge),
                            force = Mathf.Lerp(DischargeRailgunSingle.minForce, DischargeRailgunSingle.maxForce, charge),
                            falloffModel = BulletAttack.FalloffModel.None,
                            tracerEffectPrefab = DischargeRailgunSingle.tracerEffectPrefab,
                            muzzleName = "Muzzle",
                            hitEffectPrefab = DischargeRailgunSingle.hitEffectPrefab,
                            isCrit = base.RollCrit(),
                            HitEffectNormal = true,
                            radius = Mathf.Lerp(DischargeRailgunSingle.minBulletRadius, DischargeRailgunSingle.maxBulletRadius, charge),
                            smartCollision = true,
                            maxDistance = 2000f,
                            stopperMask = LayerIndex.world.mask,
                            damageType = DamageType.Generic
                        }.Fire();
                        //Add backwards self-propulsion as well? Should not scale with charge.
                    }
                    return;
                }
            }
            Util.PlaySound("Play_SniperClassicNemesis_RailgunOverheat", base.gameObject);
            //Todo: Show animation of railgun failing to open
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
