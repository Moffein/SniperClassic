using RoR2;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class HeavySnipe : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = HeavySnipe.baseDuration;
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

            Util.PlaySound(HeavySnipe.attackSoundString, base.gameObject);
            if (charge > 0f)
            {
                Util.PlaySound(HeavySnipe.chargedAttackSoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 4f, false);

            string animString = "FireGun";
            bool _isCrit = base.RollCrit();
            if (_isCrit) animString += "Crit";
            if (charge > 0f) animString = "FireGunStrong";

            base.PlayAnimation("Gesture, Override", animString);//, "FireGun.playbackRate", this.duration * 3f);

            EffectManager.SimpleMuzzleFlash(HeavySnipe.effectPrefab, base.gameObject, "Muzzle", false);

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
                    damage = this.reloadDamageMult * HeavySnipe.damageCoefficient * chargeMult * this.damageStat,
                    force = HeavySnipe.force * chargeMult * reloadDamageMult,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = HeavySnipe.tracerEffectPrefab,
                    muzzleName = "Muzzle",
                    hitEffectPrefab = HeavySnipe.hitEffectPrefab,
                    isCrit = _isCrit,
                    HitEffectNormal = true,
                    radius = HeavySnipe.radius * chargeMult,
                    smartCollision = true,
                    maxDistance = 2000f,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic,
                    stopperMask = LayerIndex.world.mask
                }.Fire();
            }

            base.AddRecoil(-1f * HeavySnipe.recoilAmplitude, -2f * HeavySnipe.recoilAmplitude, -0.5f * HeavySnipe.recoilAmplitude, 0.5f * HeavySnipe.recoilAmplitude);
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
                this.outer.SetNextState(new ReloadSnipe() { buttonReleased = skill1Released, reloadBarLength = reloadBarLength });
                return;
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

        public float reloadDamageMult = 1f;
        public float charge = 0f;

        private SniperClassic.ScopeController scopeComponent;
        private SniperClassic.ReloadController reloadComponent;
        private float duration;

        public static float damageCoefficient = 4.8f;
        public static float radius = 0.4f;
        public static float force = 750f;
        public static float baseDuration = 0.4f;

        public static float reloadBarLength = 1.4f;

        public static string attackSoundString = "Play_SniperClassic_m1_heavy";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_shoot";
        public static GameObject tracerEffectPrefab;
        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static float recoilAmplitude = 2.5f;

        public static float maxChargeMult = 3f;

        public static float baseChargeDuration = 3f;
    }
}