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
            bool isScoped = false;
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired();
                isScoped = scopeComponent.IsScoped;
            }

            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (reloadComponent)
            {
                reloadDamageMult = reloadComponent.GetDamageMult();
                reloadComponent.hideLoadIndicator = true;
                reloadComponent.brReload = false;
            }

            Util.PlaySound(internalAttackSoundString, base.gameObject);
            if ((base.isAuthority && charge > 0f) || (!base.isAuthority && scopeComponent.chargeShotReady))
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
                //base.characterBody.AddSpreadBloom(0.4f * internalRecoilAmplitude);
            }
            float adjustedRecoil = internalRecoilAmplitude * (isScoped ? 0.33f : 1f);
            base.AddRecoil(-1f * adjustedRecoil, -2f * internalRecoilAmplitude, -0.5f * adjustedRecoil, 0.5f * adjustedRecoil);
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
                        reloadComponent.EnableReloadBar(internalReloadBarLength, false);
                        this.primarySkillSlot.SetSkillOverride(this, internalReloadDef, GenericSkill.SkillOverridePriority.Contextual);
                        return;
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.primarySkillSlot)
            {
                this.primarySkillSlot.UnsetSkillOverride(this, internalReloadDef, GenericSkill.SkillOverridePriority.Contextual);
            }
            if(reloadComponent)
            {
                reloadComponent.hideLoadIndicator = false;
            }
            /*if (!reloadComponent.isReloading)
            {
                Debug.Log("running code");
                reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Bad, false);
                reloadComponent.DisableReloadBar();
            }?*/
        }

        public void AutoReload()
        {
            startedReload = true;
            if (reloadComponent)
            {
                reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect, false);
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
        private bool finishedReload = false;

        protected float internalDamage;
        protected float internalRadius;
        protected float internalForce;
        protected float internalBaseDuration;
        protected string internalAttackSoundString;
        protected string internalChargedAttackSoundString;
        protected float internalRecoilAmplitude;
        protected float inernalBaseChargeDuration;
        protected SkillDef internalReloadDef;
        protected float internalReloadBarLength;

        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
    }
}
