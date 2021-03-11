using RoR2;
using RoR2.Skills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    public abstract class BaseReloadState : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            SetStats();
            this.duration = internalScaleReloadSpeed ? internalBaseDuration / this.attackSpeedStat : internalBaseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }
            if (reloadComponent)
            {
                reloadComponent.Reload(this.duration, false, false, true);
            }
            base.PlayAnimation("Reload, Override", "Reload", "Reload.playbackRate", 0.5f);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public virtual void AutoReload()
        {
            reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect, false);
            OnExit();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (scopeComponent)
            {
                scopeComponent.ResetCharge();
                scopeComponent.pauseCharge = false;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public abstract void SetStats();

        private float duration;

        private ReloadController reloadComponent;
        private ScopeController scopeComponent;

        protected float internalBaseDuration;
        protected bool internalScaleReloadSpeed = false;
    }
}
