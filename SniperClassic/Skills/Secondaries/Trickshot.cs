using RoR2;
using SniperClassic;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    class Trickshot : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(CombatRoll.dodgeSoundString, base.gameObject);
            Util.PlaySound("Play_item_proc_crit_cooldown", base.gameObject);
            if (base.isAuthority)
            {
                if (base.characterMotor)
                {
                    if(base.characterMotor.isGrounded)
                    {
                        base.characterMotor.rootMotion += Vector3.up;
                    }
                    base.SmallHop(base.characterMotor, 17f);
                }
                TriggerReload();
            }

            if (NetworkServer.active && base.characterBody)
            {
                int buffCount = base.characterBody.GetBuffCount(SniperContent.trickshotBuff);
                base.characterBody.ClearTimedBuffs(SniperContent.trickshotBuff);
                for (int i = 0; i < buffCount + 1; i++)
                {
                    base.characterBody.AddTimedBuff(SniperContent.trickshotBuff, buffDuration + duration);
                }
            }

            //TODO: Add 360 noscope animation
            //TODO: Add VFX (spin lines + lens flare)
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= Trickshot.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private void TriggerReload()
        {
            ReloadController rc = base.characterBody.GetComponent<ReloadController>();
            if (rc)
            {
                rc.AutoReload();
            }
        }

        public static float buffDuration = 1.5f;
        public static float duration = 0.4f;
    }
}
