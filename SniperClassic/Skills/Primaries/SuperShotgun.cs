using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class SuperShotgun : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = SuperShotgun.baseDuration / this.attackSpeedStat;
            this.duration = SuperShotgun.baseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            if (scopeComponent)
            {
                charge = scopeComponent.ShotFired();
                if (scopeComponent.IsScoped())
                {
                    isScoped = true;
                }
            }

            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (reloadComponent)
            {
                reloadDamageMult = reloadComponent.GetDamageMult();
                reloadComponent.hideLoadIndicator = true;
            }

            if (isScoped)
            {
                Util.PlaySound(SuperShotgun.chargedAttackSoundString, base.gameObject);
            }
            else
            {
                Util.PlaySound(SuperShotgun.attackSoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            if (base.isAuthority)
            {
                float chargeMult = Mathf.Lerp(1f, SniperClassic.ScopeController.maxChargeMult, this.charge);
                /*new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = SuperShotgun.maxSpread * this.reloadDamageMult / chargeMult,
                    bulletCount = (uint)Mathf.RoundToInt(SuperShotgun.pelletCount * this.reloadDamageMult),
                    procCoefficient = SuperShotgun.procCoefficient,
                    damage = chargeMult * SuperShotgun.damageCoefficient * this.damageStat,
                    force = chargeMult * SuperShotgun.force,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    tracerEffectPrefab = SuperShotgun.tracerEffectPrefab,
                    muzzleName = "",
                    hitEffectPrefab = SuperShotgun.hitEffectPrefab,
                    isCrit = RollCrit(),
                    HitEffectNormal = true,
                    radius = SuperShotgun.radius,
                    smartCollision = true,
                    maxDistance = 120f,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic,
                    stopperMask = LayerIndex.world.mask
                }.Fire();*/
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = isScoped ? 0f : SuperShotgun.maxSpread * this.reloadDamageMult,
                    bulletCount = isScoped ? 1u : (uint)Mathf.RoundToInt(SuperShotgun.pelletCount * this.reloadDamageMult),
                    procCoefficient = SuperShotgun.procCoefficient,
                    damage = isScoped ? chargeMult * Mathf.Round(SuperShotgun.pelletCount * this.reloadDamageMult) * SuperShotgun.damageCoefficient * this.damageStat : SuperShotgun.damageCoefficient * this.damageStat,
                    force = isScoped ? SuperShotgun.force * chargeMult * Mathf.Round(SuperShotgun.pelletCount * this.reloadDamageMult) : SuperShotgun.force,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    tracerEffectPrefab = SuperShotgun.tracerEffectPrefab,
                    muzzleName = "",
                    hitEffectPrefab = SuperShotgun.hitEffectPrefab,
                    isCrit = RollCrit(),
                    HitEffectNormal = true,
                    radius = isScoped ? SuperShotgun.radius * chargeMult * 2f : SuperShotgun.radius,
                    smartCollision = true,
                    maxDistance = isScoped ? 240f : 120f,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic
                }.Fire();
                base.characterBody.SetSpreadBloom(SuperShotgun.maxSpread * this.reloadDamageMult / chargeMult, false);
            }

            base.AddRecoil(-1f * SuperShotgun.recoilAmplitude, -2f * SuperShotgun.recoilAmplitude, -0.5f * SuperShotgun.recoilAmplitude, 0.5f * SuperShotgun.recoilAmplitude);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.duration)
            {
                bool skill1Released = false;
                if (base.inputBank)
                {
                    skill1Released = !base.inputBank.skill1.down;
                }
                this.outer.SetNextState(new ReloadSuperShotgun() { buttonReleased = skill1Released });
                return;
            }
        }

        public void AutoReload()
        {
            if (reloadComponent)
            {
                reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect);
                reloadComponent.hideLoadIndicator = false;
            }
            this.outer.SetNextStateToMain();
            return;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public float reloadDamageMult = 1f;
        public float charge = 0f;

        private SniperClassic.ScopeController scopeComponent;
        private SniperClassic.ReloadController reloadComponent;
        private float duration;
        private bool isScoped = false;

        public static float damageCoefficient = 0.5f;
        public static uint pelletCount = 8;
        public static float procCoefficient = 0.75f;
        public static float radius = 0.3f;
        public static float force = 200f;
        public static float baseDuration = 0.4f;
        public static float maxSpread = 3f;
        public static string attackSoundString = "Play_captain_m1_shotgun_shootTight";
        public static string chargedAttackSoundString = "Play_captain_m1_shootWide";
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracerbanditshotgun");
        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static float recoilAmplitude = 2.5f;

        public static float baseChargeDuration = 3f;
    }
}
