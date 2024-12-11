using RoR2;
using SniperClassic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using RoR2.Orbs;
using System.Linq;

namespace EntityStates.SniperClassicSkills
{
    public class Backflip : BaseSkillState
    {
        public static float duration = 0.4f;
        public static float stunRadius = 12f;
        public static GameObject stunEffectPrefab;
        public static GameObject shockEffectPrefab;

        private Vector3 stunPosition;

        private float previousAirControl;

        public override void OnEnter()
        {
            base.OnEnter();
            this.previousAirControl = base.characterMotor.airControl;

            base.characterMotor.airControl = 0.15f;

            Vector3 direction = -base.GetAimRay().direction;

            if (base.isAuthority)
            {

                direction.y = Mathf.Max(direction.y, 0.05f);
                Vector3 a = direction.normalized * 4f * 10f;
                Vector3 b = Vector3.up * 7f;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * 3f;

                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = a + b + b2;
                base.characterMotor.velocity.y *= 0.8f;
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y *= 0.1f;

                base.characterBody.isSprinting = true;
                TriggerReload();

                stunPosition = base.characterBody.corePosition;
            }

            base.characterDirection.moveVector = direction;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.PlayAnimation("FullBody, Override", "Backflip", "Backflip.playbackRate", 1.5f * Backflip.duration);
            RoR2.Util.PlayAttackSpeedSound(EntityStates.Commando.DodgeState.dodgeSoundString, base.gameObject, 1.5f);


            if (NetworkServer.active)
            {
                if (stunRadius > 0f)
                {
                    StunEnemies(stunPosition);
                }
            }
        }

        private void StunEnemies(Vector3 stunPosition)
        {
            if (base.characterBody)
            {
                if(base.characterBody.coreTransform)
                {
                    EffectManager.SimpleEffect(Backflip.stunEffectPrefab, stunPosition, characterBody.coreTransform.rotation, true);
                }

                List<HealthComponent> hcList = new List<HealthComponent>();
                Collider[] array = Physics.OverlapSphere(stunPosition, Backflip.stunRadius, LayerIndex.entityPrecise.mask);
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

        // Token: 0x060003C5 RID: 965 RVA: 0x0000F87A File Offset: 0x0000DA7A
        public override void OnSerialize(NetworkWriter writer) {
            writer.Write(this.stunPosition);
        }

        // Token: 0x060003C6 RID: 966 RVA: 0x0000F888 File Offset: 0x0000DA88
        public override void OnDeserialize(NetworkReader reader) {
            this.stunPosition = reader.ReadVector3();
        }
    }
}