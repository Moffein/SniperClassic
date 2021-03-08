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
            reloadComponent.EnableReloadBar();
            reloadComponent.failedReload = false;
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!triggeredReload)
            {
                float toAdd = internalScaleReloadSpeed ? Time.deltaTime * this.attackSpeedStat : Time.deltaTime;
                while (toAdd > internalReloadBarLength)
                {
                    toAdd -= internalReloadBarLength;
                }
                if (barGoingForward)
                {
                    toAdd += this.reloadTimer;
                    if (toAdd < internalReloadBarLength)
                    {
                        this.reloadTimer = toAdd;
                    }
                    else
                    {
                        barGoingForward = false;
                        this.reloadTimer = internalReloadBarLength + internalReloadBarLength - toAdd;
                    }
                }
                else
                {
                    toAdd = this.reloadTimer - toAdd;
                    if (toAdd > 0f)
                    {
                        this.reloadTimer = toAdd;
                    }
                    else
                    {
                        barGoingForward = true;
                        this.reloadTimer = Mathf.Abs(toAdd);
                    }
                }

                reloadComponent.UpdateReloadBar(this.reloadTimer / internalReloadBarLength);

                if (!buttonReleased)
                {
                    if (!base.inputBank.skill1.down)
                    {
                        buttonReleased = true;
                    }
                }
                else
                {
                    if (base.inputBank.skill1.down)
                    {
                        triggeredReload = true;
                        DoReload();
                    }
                }
            }
            else
            {
                reloadFinishTimer += Time.deltaTime;
                if (reloadFinishTimer > this.duration)
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public virtual void DoReload()
        {
            SniperClassic.ReloadController.ReloadQuality r;
            float reloadPercent = this.reloadTimer / internalReloadBarLength;
            if (reloadPercent >= BaseReloadState.reloadBarPerfectBeginPercent && reloadPercent < BaseReloadState.reloadBarGoodBeginPercent)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Perfect;
            }
            else if (reloadPercent >= BaseReloadState.reloadBarGoodBeginPercent && reloadPercent <= BaseReloadState.reloadBarGoodEndPercent)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Good;
            }
            else
            {
                r = SniperClassic.ReloadController.ReloadQuality.Bad;
            }
            reloadComponent.SetReloadQuality(r);
            reloadComponent.hideLoadIndicator = false;

            base.PlayAnimation("Reload, Override", "Reload", "Reload.playbackRate", 0.5f);
        }

        public virtual void AutoReload()
        {
            triggeredReload = true;
            reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect, false);
            OnExit();
        }

        public override void OnExit()
        {
            base.OnExit();
            reloadComponent.DisableReloadBar();
            if (scopeComponent)
            {
                scopeComponent.ResetCharge();
                scopeComponent.pauseCharge = false;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
        public abstract void SetStats();

        internal const float reloadBarPerfectBeginPercent = 0.15f / 0.6f;
        internal const float reloadBarGoodBeginPercent = 0.255f / 0.6f;
        internal const float reloadBarGoodEndPercent = 0.38f / 0.6f;

        private float duration;
        private bool barGoingForward = true;
        private bool triggeredReload = false;
        public float reloadTimer = 0f;
        private float reloadFinishTimer = 0f;

        private ReloadController reloadComponent;
        private ScopeController scopeComponent;

        protected float internalBaseDuration;
        protected bool internalScaleReloadSpeed = false;
        protected float internalReloadBarLength;
    }
}
