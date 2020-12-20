using EntityStates.Toolbot;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    public class AimSmokeGrenade : AimThrowableBase //Credits to EnforcerGang for this code
    {
        private AimStunDrone goodState;

        public override void OnEnter()
        {
            if (goodState == null) goodState = Instantiate(typeof(AimStunDrone)) as AimStunDrone;

            maxDistance = 48;
            rayRadius = 2f;
            arcVisualizerPrefab = goodState.arcVisualizerPrefab;
            projectilePrefab = FireSmokeGrenade.projectilePrefab;
            endpointVisualizerPrefab = goodState.endpointVisualizerPrefab;
            endpointVisualizerRadiusScale = 4f;
            setFuse = false;
            damageCoefficient = 0f;
            baseMinimumDuration = 0.1f;
            projectileBaseSpeed = 80;

            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.characterBody.SetAimTimer(0.25f);
            this.fixedAge += Time.fixedDeltaTime;

            bool flag = false;

            if (base.isAuthority && !this.KeyIsDown() && base.fixedAge >= this.minimumDuration) flag = true;
            if (base.characterBody && base.characterBody.isSprinting) flag = true;

            if (flag)
            {
                this.UpdateTrajectoryInfo(out this.currentTrajectoryInfo);

                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();

            base.AddRecoil(-2f * FireSmokeGrenade.bulletRecoil, -3f * FireSmokeGrenade.bulletRecoil, -1f * FireSmokeGrenade.bulletRecoil, 1f * FireSmokeGrenade.bulletRecoil);
            base.characterBody.AddSpreadBloom(0.33f * FireSmokeGrenade.bulletRecoil);
            EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, FireSmokeGrenade.muzzleString, false);
        }
    }

    public class FireSmokeGrenade : BaseSkillState  //Credits to EnforcerGang for this code
    {
        public static float baseDuration = 0.5f;
        public static float blastRadius = 4f;
        public static float bulletRecoil = 1f;

        public static string muzzleString = "";
        public static GameObject projectilePrefab;

        private float duration;
        private ChildLocator childLocator;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FireSmokeGrenade.baseDuration / this.attackSpeedStat;
            this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
            this.animator = base.GetModelAnimator();
            Util.PlaySound("Play_commando_M2_grenade_throw", base.gameObject);

            base.AddRecoil(-2f * FireSmokeGrenade.bulletRecoil, -3f * FireSmokeGrenade.bulletRecoil, -1f * FireSmokeGrenade.bulletRecoil, 1f * FireSmokeGrenade.bulletRecoil);
            base.characterBody.AddSpreadBloom(0.33f * FireSmokeGrenade.bulletRecoil);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo info = new FireProjectileInfo()
                {
                    crit = false,
                    damage = 0,
                    damageColorIndex = DamageColorIndex.Default,
                    damageTypeOverride = DamageType.Stun1s,
                    force = 0,
                    owner = base.gameObject,
                    position = childLocator.FindChild(FireSmokeGrenade.muzzleString).position,
                    procChainMask = default(ProcChainMask),
                    projectilePrefab = FireSmokeGrenade.projectilePrefab,
                    rotation = Quaternion.LookRotation(base.GetAimRay().direction),
                    useFuseOverride = false,
                    useSpeedOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
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
