using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class AIReload : BaseState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if  (base.fixedAge > AIReload.delay)
            {
                if (!triggeredReload)
                {
                    triggeredReload = true;
                    ReloadController rl = base.GetComponent<ReloadController>();
                    if (rl)
                    {
                        if (base.isAuthority)
                        {
                            rl.SetReloadQuality(ReloadController.ReloadQuality.Perfect, true);
                        }
                        base.PlayAnimation("Reload, Override", "ReloadGunFull", "Reload.playbackRate", 0.5f);
                    }
                }
                else if (base.isAuthority && base.fixedAge > AIReload.delay + AIReload.boltDuration)
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.skillLocator && base.skillLocator.primary)
            {
                base.skillLocator.primary.stock = base.skillLocator.primary.maxStock;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static float delay = 0.4f;
        private bool triggeredReload = false;
        public static float boltDuration = 0.5f;
    }
}
