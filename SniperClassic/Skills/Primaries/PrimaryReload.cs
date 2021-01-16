using EntityStates.Commando.CommandoWeapon;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EntityStates.SniperClassicSkills
{
    class ReloadSnipe : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = ReloadSnipe.baseDuration / this.attackSpeedStat;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            reloadComponent.EnableReloadBar();
            reloadComponent.failedReload = false;
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }

            this.originalPrimaryIcon = base.skillLocator.primary.icon;
            base.skillLocator.primary.skillDef.SetFieldValue<Sprite>("icon", ReloadSnipe.reloadIcon);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!triggeredReload)
            {
                float toAdd = ReloadSnipe.scaleReloadSpeed ? Time.deltaTime * this.attackSpeedStat : Time.deltaTime;
                while (toAdd > ReloadSnipe.reloadBarLength)
                {
                    toAdd -= ReloadSnipe.reloadBarLength;
                }
                if (barGoingForward)
                {
                    toAdd += this.reloadTimer;
                    if (toAdd < ReloadSnipe.reloadBarLength)
                    {
                        this.reloadTimer = toAdd;
                    }
                    else
                    {
                        barGoingForward = false;
                        this.reloadTimer = ReloadSnipe.reloadBarLength + ReloadSnipe.reloadBarLength - toAdd;
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

                reloadComponent.UpdateReloadBar(this.reloadTimer / ReloadSnipe.reloadBarLength);

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
            if (this.reloadTimer >= ReloadSnipe.reloadBarPerfectBegin && this.reloadTimer < ReloadSnipe.reloadBarGoodBegin)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Perfect;
            }
            else if (this.reloadTimer >= ReloadSnipe.reloadBarGoodBegin && this.reloadTimer <= ReloadSnipe.reloadBarGoodEnd)
            {
                r = SniperClassic.ReloadController.ReloadQuality.Good;
            }
            else
            {
                r = SniperClassic.ReloadController.ReloadQuality.Bad;
            }
            reloadComponent.SetReloadQuality(r);
            reloadComponent.hideLoadIndicator = false;

            base.PlayAnimation("Gesture, Override", "Reload", "Reload.playbackRate", 0.5f);
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
            if (base.skillLocator && base.skillLocator.primary)
            {
                base.skillLocator.primary.skillDef.SetFieldValue<Sprite>("icon", originalPrimaryIcon);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public static float reloadBarLength = 0.6f;
        internal const float reloadBarPerfectBegin = 0.15f;
        internal const float reloadBarGoodBegin = 0.255f;
        internal const float reloadBarGoodEnd = 0.38f;

        public bool buttonReleased = false;
        private float duration;
        private bool barGoingForward = true;
        private bool triggeredReload = false;
        public float reloadTimer = 0f;
        private float reloadFinishTimer = 0f;
        public SniperClassic.ScopeController scopeComponent;
        public SniperClassic.ReloadController reloadComponent;
        private Sprite originalPrimaryIcon;

        public static float baseDuration = 0.4f;
        public static bool scaleReloadSpeed = false;
        public static Sprite reloadIcon;


        /*private float reloadFractionBadStart = reloadBarPerfectBegin / reloadBarLength;
        private float reloadFractionPerfect = (reloadBarGoodBegin - reloadBarPerfectBegin);
        private float reloadFractionGood = (reloadBarGoodEnd - reloadBarGoodBegin) / reloadBarLength;
        private float reloadFractionBadEnd = (reloadBarLength - reloadBarGoodEnd) / reloadBarLength;*/

        /*
        private Color colorReloadBorder = new Color(167f / 255f, 111f / 255f, 200f / 255f);
        private Color colorReloadPerfect = new Color(250f / 255f, 232f / 255f, 253f / 255f);
        private Color colorReloadGood = new Color(178f / 255f, 132f / 255f, 191f / 255f);
        private Color colorBar = Color.white;*/
    }
}
