using EntityStates;
using RoR2;
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

            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired(false);
            }
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (reloadComponent)
            {
                reloadComponent.hideLoadIndicator = true;
            }

            base.AddRecoil(-1f * FireBattleRifle.recoilAmplitude, -2f * FireBattleRifle.recoilAmplitude, -0.5f * FireBattleRifle.recoilAmplitude, 0.5f * FireBattleRifle.recoilAmplitude);
            Util.PlaySound(charge > 0f ? FireBattleRifle.chargedAttackSoundString : FireBattleRifle.attackSoundString, base.gameObject);
            if (base.characterBody.skillLocator.primary.stock > 0)
            {
                this.maxDuration = FireBattleRifle.baseMaxDuration / this.attackSpeedStat;
                this.minDuration = FireBattleRifle.baseMinDuration / this.attackSpeedStat;
                base.characterBody.skillLocator.primary.rechargeStopwatch = 0f;
            }
            else//handle reload in here
            {
                this.lastShot = true;
                Util.PlaySound(FireBattleRifle.emptySoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            if (base.characterBody.skillLocator.primary.baseRechargeInterval <= 0f)
            {
                base.PlayAnimation("Gesture, Additive", "FireShotgun", "FireShotgun.playbackRate", 1.1f);
                base.PlayAnimation("Gesture, Override", "FireShotgun", "FireShotgun.playbackRate", 1.1f);
            }
            else
            {
                base.PlayAnimation("Gesture, Additive", "FireShotgun", "FireShotgun.playbackRate", base.characterBody.skillLocator.primary.CalculateFinalRechargeInterval() * 1.1f);
                base.PlayAnimation("Gesture, Override", "FireShotgun", "FireShotgun.playbackRate", base.characterBody.skillLocator.primary.CalculateFinalRechargeInterval() * 1.1f);
            }
            string muzzleName = "MuzzleShotgun";
            if (FireBattleRifle.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireBattleRifle.effectPrefab, base.gameObject, muzzleName, false);
            }
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
                    damage = FireBattleRifle.damageCoefficient * this.damageStat * chargeMult,
                    force = FireBattleRifle.force * chargeMult,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = FireBattleRifle.tracerEffectPrefab,
                    muzzleName = muzzleName,
                    hitEffectPrefab = FireBattleRifle.hitEffectPrefab,
                    isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    HitEffectNormal = true,
                    radius = FireBattleRifle.radius * chargeMult,
                    smartCollision = true,
                    maxDistance = 999f,
                    stopperMask = LayerIndex.world.mask,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic
                }.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
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
                    this.outer.SetNextState(new ReloadBattleRifle() { buttonReleased = this.buttonReleased });
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
            if (reloadComponent)
            {
                reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect);
                base.skillLocator.primary.stock = reloadComponent.GetMagSize();
                reloadComponent.hideLoadIndicator = true;
            }
            this.outer.SetNextStateToMain();
            return;
        }

        private SniperClassic.ScopeController scopeComponent;
        private SniperClassic.ReloadController reloadComponent;
        public float charge = 0f;

        public static GameObject tracerEffectPrefab = EntityStates.Sniper.SniperWeapon.FireRifle.tracerEffectPrefab;
        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");

        public static float damageCoefficient = 3.2f;
        public static float force = 250f;
        public static float baseMinDuration = 0.33f;
        public static float baseMaxDuration = 0.5f;
        public static float radius = 0.4f;

        public static string attackSoundString = "Play_bandit_M2_shot";
        public static string chargedAttackSoundString = "Play_item_use_lighningArm";
        public static string emptySoundString = "Play_commando_M2_grenade_throw";
        public static float recoilAmplitude = 3f;
        public static float spreadBloomValue = 0.3f;

        public static float baseChargeDuration = 1.5f;

        private float maxDuration;
        private float minDuration;
        private bool buttonReleased;
        public bool isMash = false;
        private bool lastShot = false;
    }
}
