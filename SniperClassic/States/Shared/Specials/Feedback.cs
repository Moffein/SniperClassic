using RoR2;
using RoR2.Skills;
using SniperClassic;
using UnityEngine;

namespace EntityStates.SniperClassicSkills
{
    class SendSpotter : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			SetSpotterMode();
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
					spotterTargetingController.ClientSendSpotter(spotterMode);
					Util.PlaySound(SendSpotter.attackSoundString, base.gameObject);
				}

				this.specialSkillSlot.SetSkillOverride(this, SendSpotter.specialSkillDef, GenericSkill.SkillOverridePriority.Contextual);

                if (!base.characterBody.isSprinting) base.StartAimMode(1f, false);
                
				base.GetModelAnimator().Play("SpotterOn"); //not using play animation cause it's just a transition state
				//base.PlayAnimation("Spotter, Override", "SpotterOn");
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
				//this.specialSkillSlot.DeductStock(1);
			}
			else
            {
				//this.specialSkillSlot.AddOneStock();
			}
			base.OnExit();
		}

		public virtual void SetSpotterMode()
        {
			spotterMode = SpotterMode.ChainLightning;
		}

		private SpotterTargetingController spotterTargetingController = null;

		public static SkillDef specialSkillDef;
		public static float baseExitDuration = 0f;
		public static string attackSoundString = "Play_SniperClassic_spotter";

		private GenericSkill specialSkillSlot;
		private float timerSinceComplete;
		private bool beginExit;
		private bool foundTarget = false;

		public SpotterMode spotterMode;
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

            if (!base.characterBody.isSprinting) base.StartAimMode(1f, false);

            base.GetModelAnimator().Play("SpotterOff"); //not using play animation cause it's just a transition state
            //base.PlayAnimation("Spotter, Override", "SpotterOff");
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
