using RoR2;
using RoR2.Skills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    class SendSpotter : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			this.spotterTargetingController = base.gameObject.GetComponent<SpotterTargetingController>();
			if (this.spotterTargetingController)
			{
				if (spotterTargetingController.GetTrackingTarget())
                {
					foundTarget = true;
                }
			}

			this.specialSkillSlot = (base.skillLocator ? base.skillLocator.special : null);
			if (foundTarget && this.specialSkillSlot)
			{
				if (base.isAuthority)
                {
					spotterTargetingController.ClientSendSpotter();
					Util.PlaySound(SendSpotter.attackSoundString, base.gameObject);
				}

				this.specialSkillSlot.SetSkillOverride(this, SendSpotter.specialSkillDef, GenericSkill.SkillOverridePriority.Contextual);

                base.PlayAnimation("Gesture, Override", "SpotterOn", "Spotter.playbackRate", 1f);
            }
			else
            {
				OnExit();
            }
        }

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (!this.specialSkillSlot || this.specialSkillSlot.stock == 0)
			{
				this.beginExit = true;
			}
			if (this.beginExit)
			{
				this.timerSinceComplete += Time.fixedDeltaTime;
				if (this.timerSinceComplete > SendSpotter.baseExitDuration)
				{
					this.outer.SetNextStateToMain();
				}
			}
		}

		public override void OnExit()
		{
			if (foundTarget)
            {
				if (this.specialSkillSlot)
				{
					this.specialSkillSlot.UnsetSkillOverride(this, SendSpotter.specialSkillDef, GenericSkill.SkillOverridePriority.Contextual);
				}
				this.specialSkillSlot.DeductStock(1);
			}
			else
            {
				this.specialSkillSlot.AddOneStock();
			}
			base.OnExit();
		}

		private SpotterTargetingController spotterTargetingController = null;

		public static SkillDef specialSkillDef;
		public static float baseExitDuration = 0f;
		public static string attackSoundString = "Play_SniperClassic_spotter";

		private GenericSkill specialSkillSlot;
		private float timerSinceComplete;
		private bool beginExit;
		private bool foundTarget = false;
	}

    class ReturnSpotter : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
			this.spotterTargetingController = base.gameObject.GetComponent<SpotterTargetingController>();
			if (this.spotterTargetingController && base.isAuthority)
			{
				spotterTargetingController.ClientReturnSpotter();
			}

            base.PlayAnimation("Gesture, Override", "SpotterOff", "Spotter.playbackRate", 1f);
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (base.fixedAge > SendSpotter.baseExitDuration)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
        {
            base.OnExit();
        }

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		public static float baseExitDuration = 0.5f;
		private SpotterTargetingController spotterTargetingController = null;
	}
}
