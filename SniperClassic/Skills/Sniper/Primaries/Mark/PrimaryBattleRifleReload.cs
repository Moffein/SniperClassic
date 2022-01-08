using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    class ReloadBR : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = scaleReloadSpeed ? ReloadBR.baseDuration / this.attackSpeedStat : ReloadBR.baseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            base.PlayAnimation("Reload, Override", "ReloadMark", "Reload.playbackRate", 0.5f);

            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }
            if (reloadComponent)
            {
                reloadComponent.ReloadBR(this.duration, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge > this.duration && reloadComponent.finishedReload)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (scopeComponent)
            {
                scopeComponent.ResetCharge();
                scopeComponent.pauseCharge = false;
            }
            base.OnExit();
        }
        public void AutoReload()
        {
            this.outer.SetNextStateToMain();
            return;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public bool buttonReleased = false;
        private float duration;
        public SniperClassic.ScopeController scopeComponent;
        public SniperClassic.ReloadController reloadComponent;

        public static float baseDuration = 0.6f;
        public static bool scaleReloadSpeed = false;
    }
}
