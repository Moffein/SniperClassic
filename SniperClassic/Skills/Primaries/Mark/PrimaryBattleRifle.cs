using EntityStates;
using RoR2;
using RoR2.Skills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class FireBattleRifle : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.primarySkillSlot = (base.skillLocator ? base.skillLocator.primary : null);
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (reloadComponent)
            {
                reloadComponent.hideLoadIndicator = true;
            }
            float adjustedRecoil = FireBattleRifle.recoilAmplitude * (isScoped ? 1f : 1f);
            base.AddRecoil(-1f * adjustedRecoil, -2f * adjustedRecoil, -0.5f * adjustedRecoil, 0.5f * adjustedRecoil);
            if (base.characterBody.skillLocator.primary.stock > 0)
            {
                this.maxDuration = FireBattleRifle.baseMaxDuration / this.attackSpeedStat;
                this.minDuration = FireBattleRifle.baseMinDuration / this.attackSpeedStat;
                base.characterBody.skillLocator.primary.rechargeStopwatch = 0f;
            }
            else
            {
                this.lastShot = true;
                if (base.isAuthority)
                {
                    Util.PlaySound(ReloadController.pingSoundString, base.gameObject);
                    reloadComponent.CmdPlayPing();
                }
            }
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired(lastShot);
                isScoped = scopeComponent.IsScoped;
                scopeComponent.pauseCharge = true;
            }
            Util.PlaySound((base.isAuthority && charge > 0f) || (!base.isAuthority && scopeComponent.chargeShotReady) ? FireBattleRifle.chargedAttackSoundString : FireBattleRifle.attackSoundString, base.gameObject);

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 4f, false);

            string animString = "FireGunMark";
            bool _isCrit = base.RollCrit();
            if (charge > 0f) animString = "FireGunStrong";

            base.PlayAnimation("Gesture, Override", animString, "FireGun.playbackRate", this.maxDuration);

            string muzzleName = "Muzzle";
            EffectManager.SimpleMuzzleFlash(FireBattleRifle.effectPrefab, base.gameObject, muzzleName, false);
            if (base.isAuthority)
            {
                float chargeMult = Mathf.Lerp(1f, maxChargeMult, this.charge);
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
                    damage = FireBattleRifle.damageCoefficient * this.damageStat * chargeMult,
                    force = FireBattleRifle.force * chargeMult,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = SniperClassic.Modules.Assets.markTracer,
                    muzzleName = muzzleName,
                    hitEffectPrefab = FireBattleRifle.hitEffectPrefab,
                    isCrit = _isCrit,
                    HitEffectNormal = true,
                    radius = FireBattleRifle.radius * chargeMult,
                    smartCollision = true,
                    maxDistance = 2000f,
                    stopperMask = LayerIndex.world.mask,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic
                }.Fire();
                //base.characterBody.AddSpreadBloom(0.8f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (lastShot)
            {
                if (this.primarySkillSlot)
                {
                    this.primarySkillSlot.UnsetSkillOverride(this, reloadDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.buttonReleased |= !base.inputBank.skill1.down;
            if (base.fixedAge >= this.maxDuration && base.isAuthority)
            {
                if (!lastShot)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
                else
                {
                    if (!startedReload && base.skillLocator && this.primarySkillSlot)
                    {
                        startedReload = true;
                        reloadComponent.EnableReloadBar(reloadLength, true, ReloadController.reloadAttackSpeedScale ? ReloadBR.baseDuration / this.attackSpeedStat : ReloadBR.baseDuration) ;
                        this.primarySkillSlot.SetSkillOverride(this, reloadDef, GenericSkill.SkillOverridePriority.Contextual);
                        return;
                    }
                    else
                    {
                        if (reloadComponent.finishedReload)
                        {
                            this.outer.SetNextStateToMain();
                            return;
                        }
                    }
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            /*if (!lastShot && this.buttonReleased && base.fixedAge >= this.minDuration)
            {
                base.characterBody.AddSpreadBloom(FireBattleRifle.spreadBloomValue);
                return InterruptPriority.Any;
            }*/
            return InterruptPriority.Skill;
        }

        public void AutoReload()
        {
            this.outer.SetNextStateToMain();
            return;
        }

        private SniperClassic.ScopeController scopeComponent;
        private SniperClassic.ReloadController reloadComponent;
        public float charge = 0f;

        public static GameObject tracerEffectPrefab = EntityStates.Sniper.SniperWeapon.FireRifle.tracerEffectPrefab;
        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");

        public static SkillDef reloadDef;
        private GenericSkill primarySkillSlot;
        private bool startedReload = false;
        public static float reloadLength = 1.6f;

        public static float damageCoefficient = 2.8f;
        public static float force = 250f;
        public static float baseMinDuration = 0.33f;
        public static float baseMaxDuration = 0.5f;
        public static float radius = 0.4f;

        public static string attackSoundString = "Play_SniperClassic_m1_br_shoot";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_br";
        public static float recoilAmplitude = 3f;

        public static float baseChargeDuration = 2f;

        public static float maxChargeMult = 4f;

        private float maxDuration;
        private float minDuration;
        private bool buttonReleased;
        public bool isMash = false;
        private bool lastShot = false;
        private bool isScoped = false;
    }
}
