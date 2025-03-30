using EntityStates.Bandit2.Weapon;
using EntityStates.Engi.EngiWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using SniperClassic;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    public abstract class BaseSnipeState : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            isAI = !base.characterBody.isPlayerControlled;
            SetStats();
            this.primarySkillSlot = (base.skillLocator ? base.skillLocator.primary : null);
            //this.duration = BaseSnipeState.baseDuration / this.attackSpeedStat;
            this.duration = internalBaseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            bool isScoped = false;

            float chargeMult = 1f;
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired();
                chargeMult = scopeComponent.GetChargeMult(charge);
                scopeComponent.pauseCharge = true;
                isScoped = scopeComponent.IsScoped;
            }

            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            reloadDamageMult = reloadComponent.GetDamageMult();
            reloadComponent.hideLoadIndicator = true;

            if (charge > 0f)
            {
                SniperClassic.Util.HandleLuminousShotServer(characterBody);
            }

            reloadComponent.brReload = false;

            isCharged = (base.isAuthority && charge > 0.5f) || (!base.isAuthority && scopeComponent.chargeShotReady);

            RoR2.Util.PlaySound(internalAttackSoundString, base.gameObject);
            if (isCharged)
            {
                RoR2.Util.PlaySound(internalChargedAttackSoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 4f, false);
            string animString = "FireGun";
            bool _isCrit = base.RollCrit();
            if (isCharged) animString = "FireGunStrong";

            base.PlayAnimation("Gesture, Override", animString);//, "FireGun.playbackRate", this.duration * 3f);
            base.PlayAnimation("Reload, Override", "BufferEmpty");

            EffectManager.SimpleMuzzleFlash(BaseSnipeState.effectPrefab, base.gameObject, "Muzzle", false);

            if (base.isAuthority)
            {
                FireBullet(aimRay, chargeMult, _isCrit);
                base.characterBody.AddSpreadBloom(0.6f);
            }
            float adjustedRecoil = internalRecoilAmplitude * (isScoped ? 1f : 1f);
            base.AddRecoil(-1f * adjustedRecoil, -2f * internalRecoilAmplitude, -0.5f * adjustedRecoil, 0.5f * adjustedRecoil);

            reloadComponent.ResetReloadQuality();
        }

        public virtual void FireBullet(Ray aimRay, float chargeMult, bool crit)
        {
            float clampedChargeMult = Mathf.Min(chargeMult, ScopeController.baseMaxChargeMult);
            BulletAttack ba = new BulletAttack
            {
                owner = base.gameObject,
                weapon = base.gameObject,
                origin = aimRay.origin,
                aimVector = aimRay.direction,
                minSpread = 0f,
                maxSpread = 0f,
                bulletCount = 1u,
                procCoefficient = 1f,
                damage = this.reloadDamageMult * internalDamage * base.damageStat * chargeMult,
                force = internalForce,
                falloffModel = BulletAttack.FalloffModel.None,
                tracerEffectPrefab = SniperClassic.Modules.Assets.snipeTracer,
                muzzleName = "Muzzle",
                hitEffectPrefab = BaseSnipeState.hitEffectPrefab,
                isCrit = crit,
                HitEffectNormal = true,
                radius = internalRadius * clampedChargeMult,
                smartCollision = true,
                maxDistance = 2000f,
                damageType = DamageType.Generic,
                stopperMask = LayerIndex.world.mask
            };
            ba.damageType.damageSource = charge > 0f ? DamageSource.Secondary : DamageSource.Primary;

            if (chargeMult >= ScopeController.baseMaxChargeMult)
            {
                ba.AddModdedDamageType(SniperContent.FullCharge);
                if (SniperClassic.SniperClassic.enableWeakPoints)
                {
                    ba.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
                    {
                        if (BulletAttack.IsSniperTargetHit(hitInfo))
                        {
                            damageInfo.damage *= ScopeController.weakpointMultiplier;
                            damageInfo.damageColorIndex = DamageColorIndex.Sniper;

                            EffectData effectData = new EffectData
                            {
                                origin = hitInfo.point,
                                rotation = Quaternion.LookRotation(-hitInfo.direction)
                            };
                            effectData.SetHurtBoxReference(hitInfo.hitHurtBox);
                            EffectManager.SpawnEffect(BaseSnipeState.headshotEffectPrefab, effectData, true);
                            RoR2.Util.PlaySound("Play_SniperClassic_headshot", base.gameObject);
                        }
                    };
                }

                if (!(SniperClassic.SniperClassic.arenaActive && SniperClassic.SniperClassic.arenaNerf))
                {
                    ba.damageType.damageType |= DamageType.Stun1s;
                }
            }

            ba.Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.duration)
            {
                if (base.isAuthority)
                {
                    if (!startedReload)
                    {
                        startedReload = true;
                        if (!isAI && base.skillLocator && this.primarySkillSlot)
                        {
                            reloadComponent.EnableReloadBar(internalReloadBarLength, false);
                            EnterReloadAnimation();
                            this.primarySkillSlot.SetSkillOverride(this, internalReloadDef, GenericSkill.SkillOverridePriority.Contextual);
                            return;
                        }
                        else
                        {
                            this.outer.SetNextState(new AIReload());
                            return;
                        }
                    }
                }
            }
        }

        public virtual void EnterReloadAnimation()
        {
            PlayAnimation("Reload, Override", "ReloadGunReady");
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.primarySkillSlot && !isAI)
            {
                this.primarySkillSlot.UnsetSkillOverride(this, internalReloadDef, GenericSkill.SkillOverridePriority.Contextual);
            }
            if(reloadComponent)
            {
                reloadComponent.hideLoadIndicator = false;
            }
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = false;
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
        private bool isAI = false;
        private bool isCharged = false;

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

        public static GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject headshotEffectPrefab;
    }
}
