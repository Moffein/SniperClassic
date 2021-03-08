using RoR2;
using RoR2.Skills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    public abstract class BaseSnipeState : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            SetStats();
            this.primarySkillSlot = (base.skillLocator ? base.skillLocator.primary : null);
            //this.duration = BaseSnipeState.baseDuration / this.attackSpeedStat;
            this.duration = internalBaseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired();
            }

            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (reloadComponent)
            {
                reloadDamageMult = reloadComponent.GetDamageMult();
                reloadComponent.hideLoadIndicator = true;
                reloadComponent.brReload = false;
            }

            Util.PlaySound(internalAttackSoundString, base.gameObject);
            if (charge > 0f)
            {
                Util.PlaySound(internalChargedAttackSoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 4f, false);

            string animString = "FireGun";
            bool _isCrit = base.RollCrit();
            if (charge > 0f) animString = "FireGunStrong";

            base.PlayAnimation("Gesture, Override", animString);//, "FireGun.playbackRate", this.duration * 3f);

            EffectManager.SimpleMuzzleFlash(BaseSnipeState.effectPrefab, base.gameObject, "Muzzle", false);

            if (base.isAuthority)
            {
                float chargeMult = Mathf.Lerp(1f, ScopeController.maxChargeMult, this.charge);
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
                    damage = this.reloadDamageMult * internalDamage * chargeMult * this.damageStat,
                    force = internalForce * chargeMult * reloadDamageMult,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = SniperClassic.Modules.Assets.snipeTracer,
                    muzzleName = "Muzzle",
                    hitEffectPrefab = BaseSnipeState.hitEffectPrefab,
                    isCrit = _isCrit,
                    HitEffectNormal = true,
                    radius = internalRadius * chargeMult,
                    smartCollision = true,
                    maxDistance = 2000f,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic,
                    stopperMask = LayerIndex.world.mask
                }.Fire();
            }

            base.AddRecoil(-1f * internalRecoilAmplitude, -2f * internalRecoilAmplitude, -0.5f * internalRecoilAmplitude, 0.5f * internalRecoilAmplitude);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.duration)
            {
                if (base.isAuthority)
                {
                    if (!startedReload && base.skillLocator && this.primarySkillSlot)
                    {
                        startedReload = true;
                        this.primarySkillSlot.SetSkillOverride(this, internalReloadDef, GenericSkill.SkillOverridePriority.Contextual);
                        return;
                    }
                    else
                    {
                        if (!this.primarySkillSlot || this.primarySkillSlot.stock == 0)
                        {
                            this.outer.SetNextStateToMain();
                            return;
                        }
                    }
                }
            }
        }

        public void AutoReload()
        {
            if (reloadComponent)
            {
                reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect, false);
                reloadComponent.hideLoadIndicator = false;
            }
            this.outer.SetNextStateToMain();
            return;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public abstract void SetStats();

        public float reloadDamageMult = 1f;
        public float charge = 0f;

        private ScopeController scopeComponent;
        private ReloadController reloadComponent;
        private float duration;
        private GenericSkill primarySkillSlot;
        private bool startedReload = false;

        protected float internalDamage;
        protected float internalRadius;
        protected float internalForce;
        protected float internalBaseDuration;
        protected string internalAttackSoundString;
        protected string internalChargedAttackSoundString;
        protected float internalRecoilAmplitude;
        protected float inernalBaseChargeDuration;
        protected SkillDef internalReloadDef;

        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
    }
}
