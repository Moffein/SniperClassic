using RoR2;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class Snipe : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = Snipe.baseDuration / this.attackSpeedStat;
            this.duration = Snipe.baseDuration;
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

            Util.PlaySound(Snipe.attackSoundString, base.gameObject);
            if (charge > 0f)
            {
                Util.PlaySound(Snipe.chargedAttackSoundString, base.gameObject);
            }

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 4f, false);

            string animString = "FireGun";
            bool _isCrit = base.RollCrit();
            if (charge > 0f) animString = "FireGunStrong";

            base.PlayAnimation("Gesture, Override", animString);//, "FireGun.playbackRate", this.duration * 3f);

            EffectManager.SimpleMuzzleFlash(Snipe.effectPrefab, base.gameObject, "Muzzle", false);

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
                    damage = this.reloadDamageMult * Snipe.damageCoefficient * chargeMult * this.damageStat,
                    force = Snipe.force * chargeMult * reloadDamageMult,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = SniperClassic.Modules.Assets.snipeTracer,
                    muzzleName = "Muzzle",
                    hitEffectPrefab = Snipe.hitEffectPrefab,
                    isCrit = _isCrit,
                    HitEffectNormal = true,
                    radius = Snipe.radius * chargeMult,
                    smartCollision = true,
                    maxDistance = 2000f,
                    damageType = this.charge >= 1f ? DamageType.Stun1s : DamageType.Generic,
                    stopperMask = LayerIndex.world.mask
                }.Fire();
            }

            base.AddRecoil(-1f * Snipe.recoilAmplitude, -2f * Snipe.recoilAmplitude, -0.5f * Snipe.recoilAmplitude, 0.5f * Snipe.recoilAmplitude);
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

        public void SetStats()
        {

        }

        public float reloadDamageMult = 1f;
        public float charge = 0f;

        private SniperClassic.ScopeController scopeComponent;
        private SniperClassic.ReloadController reloadComponent;
        private float duration;

        public static float damageCoefficient = 3.6f;
        public static float radius = 0.4f;
        public static float force = 500f;
        public static float baseDuration = 0.4f;

        public static float reloadBarLength = 0.6f;

        public static string attackSoundString = "Play_SniperClassic_m1_shoot";
        public static string chargedAttackSoundString = "Play_SniperClassic_m2_shoot";
        public static GameObject tracerEffectPrefab;
        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/muzzleflashbanditshotgun");
        public static float recoilAmplitude = 2.5f;

        public static float maxChargeMult = 4f;

        public static float baseChargeDuration = 3f;
    }
}
