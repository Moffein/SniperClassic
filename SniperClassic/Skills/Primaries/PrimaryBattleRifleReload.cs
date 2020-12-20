using AK.Wwise;
using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class ReloadBR : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = ReloadBR.baseDuration / this.attackSpeedStat;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            reloadComponent.brReload = true;
            reloadComponent.failedReload = false;
            reloadComponent.EnableReloadBar();

            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }

            this.originalPrimaryIcon = base.skillLocator.primary.icon;
            base.skillLocator.primary.skillDef.SetFieldValue<Sprite>("icon", ReloadBR.reloadIcon);
            base.skillLocator.primary.stock = 1;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!triggeredReload)
            {
                float toAdd = ReloadBR.scaleReloadSpeed ? Time.deltaTime * this.attackSpeedStat : Time.deltaTime;
                while (toAdd > ReloadBR.reloadBarLength)
                {
                    toAdd -= ReloadBR.reloadBarLength;
                }
                toAdd += this.reloadTimer;
                if (toAdd < ReloadBR.reloadBarLength)
                {
                    this.reloadTimer = toAdd;
                }
                else
                {
                    triggeredReload = true;
                    DoReload();
                }

                reloadComponent.UpdateReloadBar(this.reloadTimer / ReloadBR.reloadBarLength);

                if (!buttonReleased)
                {
                    if (!base.inputBank.skill1.down)
                    {
                        buttonReleased = true;
                    }
                }
                else
                {
                    if (base.inputBank.skill1.down && !failedReload)
                    {
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
            if (this.reloadTimer >= ReloadBR.reloadBarGoodStart && this.reloadTimer < ReloadBR.reloadBarGoodEnd)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Good;
                triggeredReload = true;
            }
            else if (this.reloadTimer >= ReloadBR.reloadBarGoodEnd && this.reloadTimer <= ReloadBR.reloadBarPerfectEnd)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Perfect;
                triggeredReload = true;

                if (base.skillLocator.secondary.stock < base.skillLocator.secondary.maxStock)
                {
                    base.skillLocator.secondary.AddOneStock();
                }
            }
            else
            {
                r = SniperClassic.ReloadController.ReloadQuality.Bad;
                if (!triggeredReload)
                {
                    reloadComponent.SetReloadQuality(r);
                }
                reloadComponent.failedReload = true;
                failedReload = true;
            }

            if (triggeredReload)
            {
                reloadComponent.SetReloadQuality(r);
                base.skillLocator.primary.stock = base.skillLocator.primary.maxStock;
            }
            reloadComponent.hideLoadIndicator = true;
        }

        public virtual void AutoReload()
        {
            triggeredReload = true;
            failedReload = false;
            reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Good);
            //reloadComponent.BattleRiflePerfectReload();
            this.reloadComponent.hideLoadIndicator = true;
            base.skillLocator.primary.stock = base.skillLocator.primary.maxStock;
            OnExit();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (reloadComponent)
            {
                reloadComponent.DisableReloadBar();
            }
            if (scopeComponent)
            {
                scopeComponent.ResetCharge();
                scopeComponent.pauseCharge = false;
            }
            if (base.skillLocator && base.skillLocator.primary)
            {
                base.skillLocator.primary.skillDef.SetFieldValue<Sprite>("icon", originalPrimaryIcon);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public static float reloadBarLength = 1.6f;
        internal const float reloadBarPerfectEnd = 1.18f;
        internal const float reloadBarGoodEnd = 0.9f;
        internal const float reloadBarGoodStart = 0.57f;

        public bool buttonReleased = false;
        private float duration;
        private bool triggeredReload = false;
        private bool failedReload = false;
        public float reloadTimer = 0f;
        private float reloadFinishTimer = 0f;
        public SniperClassic.ScopeController scopeComponent;
        public SniperClassic.ReloadController reloadComponent;
        private Sprite originalPrimaryIcon;

        public static float baseDuration = 0.4f;
        public static bool scaleReloadSpeed = false;
        public static Sprite reloadIcon;
    }
}
