using RoR2;
using SniperClassic.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills.Nemesis
{
    public class FireRailgunSingle : BaseState
    {
        public static float force = 1000f;
        public static float damageCoefficient = 2.6f;
        public static float heatCost = 15f;
        public static float bulletRadius = 0.4f;
        public static float baseDuration = 0.4f;
        public static GameObject tracerEffectPrefab;
        public static GameObject hitEffectPrefab;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = FireRailgunSingle.baseDuration / this.attackSpeedStat;

            RailgunHeatController rhc = base.GetComponent<RailgunHeatController>();
            if (rhc)
            {
                if (!rhc.overheated)
                {
                    Util.PlaySound("Play_SniperClassicNemesis_RailgunShoot", base.gameObject);
                    if (base.isAuthority)
                    {
                        //If the numbers are correct, at base fire rate it should take 8 consecutive shots to overheat the gun.
                        rhc.AddHeat(heatCost);
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
                            damage = this.damageStat * FireRailgunSingle.damageCoefficient,
                            force = FireRailgunSingle.force,
                            falloffModel = BulletAttack.FalloffModel.None,
                            tracerEffectPrefab = FireRailgunSingle.tracerEffectPrefab,
                            muzzleName = "Muzzle",
                            hitEffectPrefab = FireRailgunSingle.hitEffectPrefab,
                            isCrit = base.RollCrit(),
                            HitEffectNormal = true,
                            radius = FireRailgunSingle.bulletRadius,
                            smartCollision = true,
                            maxDistance = 2000f,
                            stopperMask = LayerIndex.world.mask,
                            damageType = DamageType.Generic
                        }.Fire();
                    }
                    return;
                }
            }
            Util.PlaySound("Play_SniperClassicNemesis_RailgunOverheat", base.gameObject);
            //TODO: Add gun smoke
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
            return InterruptPriority.Skill;
        }
    }
}
