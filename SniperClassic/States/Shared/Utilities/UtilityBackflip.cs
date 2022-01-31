using RoR2;
using SniperClassic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    public class Backflip : BaseSkillState
    {
        public static float duration = 0.4f;
        public static float stunRadius = 12f;
        public static GameObject stunEffectPrefab;

        private float previousAirControl;

        public override void OnEnter()
        {
            base.OnEnter();
            this.previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = EntityStates.Croco.Leap.airControl;

            Vector3 direction = -base.GetAimRay().direction;

            if (base.isAuthority)
            {
                base.characterBody.isSprinting = true;

                direction.y = Mathf.Max(direction.y, EntityStates.Croco.Leap.minimumY);
                Vector3 a = direction.normalized * EntityStates.Croco.Leap.aimVelocity * 10f;
                Vector3 b = Vector3.up * EntityStates.Croco.Leap.upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * EntityStates.Croco.Leap.forwardVelocity;

                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = a + b + b2;
                base.characterMotor.velocity.y *= 0.8f;
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y *= 0.1f;
            }

            base.characterDirection.moveVector = direction;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.PlayAnimation("FullBody, Override", "Backflip", "Backflip.playbackRate", 1.5f * Backflip.duration);
            Util.PlayAttackSpeedSound(EntityStates.Commando.DodgeState.dodgeSoundString, base.gameObject, 1.5f);

            if (base.isAuthority)
            {
                TriggerReload();
            }

            if (NetworkServer.active)
            {
                if (stunRadius > 0f)
                {
                    StunEnemies();
                }
                DistractEnemies();
            }
        }

        private void DistractEnemies()
        {
            SpotterTargetingController stc = base.GetComponent<SpotterTargetingController>();
            if (stc && stc.spotterFollower && stc.spotterFollower.distractController)
            {
                if (base.characterBody)
                {
                    stc.spotterFollower.distractController.distractPosition = base.characterBody.corePosition;
                }
                stc.spotterFollower.distractController.StartDistraction();
            }
        }

        private void StunEnemies()
        {
            if (base.characterBody)
            {
                if(base.characterBody.coreTransform)
                {
                    EffectManager.SimpleEffect(Backflip.stunEffectPrefab, base.characterBody.corePosition, base.characterBody.coreTransform.rotation, true);
                }

                List<HealthComponent> hcList = new List<HealthComponent>();
                Collider[] array = Physics.OverlapSphere(base.characterBody.corePosition, Backflip.stunRadius, LayerIndex.entityPrecise.mask);
                for (int i = 0; i < array.Length; i++)
                {
                    HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                    if (hurtBox && hurtBox.healthComponent && !hcList.Contains(hurtBox.healthComponent))
                    {
                        hcList.Add(hurtBox.healthComponent);
                        if (hurtBox.healthComponent.body.teamComponent && hurtBox.healthComponent.body.teamComponent.teamIndex != base.GetTeam())
                        {
                            SetStateOnHurt ssoh = hurtBox.healthComponent.gameObject.GetComponent<SetStateOnHurt>();
                            if (ssoh && ssoh.canBeStunned)
                            {
                                ssoh.SetStun(1f);
                            }
                        }
                    }
                }
            }
        }

        private void TriggerReload()
        {
            ReloadController rc = base.characterBody.GetComponent<ReloadController>();
            if (rc) rc.AutoReload();
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterMotor.airControl = this.previousAirControl;

            base.characterMotor.velocity *= 0.9f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.5f, false);

            if (base.fixedAge >= Backflip.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}