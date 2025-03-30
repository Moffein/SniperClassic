﻿using EntityStates.Commando.CommandoWeapon;
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
            this.duration = internalBaseDuration;
            scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
            reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
            if (scopeComponent)
            {
                scopeComponent.pauseCharge = true;
                scopeComponent.charge = 0f;
            }
            if (reloadComponent)
            {
                reloadComponent.Reload(this.duration);
            }

            PlayReloadAnimation();

        }

        private void PlayReloadAnimation()
        {
            base.PlayAnimation("Reload, Override", "ReloadGunEnd");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge > this.duration && reloadComponent.finishedReload)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public void AutoReload()
        {
            reloadComponent.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect, false);
            OnExit();
        }

        public override void OnExit()
        {
            Debug.LogWarning("RELOAD OnExit");

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
    }
}
