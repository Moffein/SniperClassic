using RoR2;
using SniperClassic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
	class CombatRoll : BaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();

			scopeController = base.gameObject.GetComponent<SniperClassic.ScopeController>();

			Util.PlaySound(CombatRoll.dodgeSoundString, base.gameObject);
			this.animator = base.GetModelAnimator();
			ChildLocator component = this.animator.GetComponent<ChildLocator>();
			if (base.isAuthority && base.inputBank && base.characterDirection)
			{
				this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
			}
			Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.forwardDirection;
			Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
			float num = Vector3.Dot(this.forwardDirection, rhs);
			float num2 = Vector3.Dot(this.forwardDirection, rhs2);

            base.PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", CombatRoll.duration);

            this.animator.SetFloat("forwardSpeed", num, 0.1f, Time.fixedDeltaTime);
			this.animator.SetFloat("rightSpeed", num2, 0.1f, Time.fixedDeltaTime);
			if (Mathf.Abs(num) > Mathf.Abs(num2))
			{
				base.PlayAnimation("Body", (num > 0f) ? "DodgeForward" : "DodgeBackward", "Dodge.playbackRate", CombatRoll.duration);
			}
			else
			{
				base.PlayAnimation("Body", (num2 > 0f) ? "DodgeRight" : "DodgeLeft", "Dodge.playbackRate", CombatRoll.duration);
			}

			this.RecalculateRollSpeed();
			if (base.characterMotor && base.characterDirection)
			{
				base.characterMotor.velocity.y = 0f;
				base.characterMotor.velocity = this.forwardDirection * this.rollSpeed;
			}
			Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
			this.previousPosition = base.transform.position - b;

			if (base.isAuthority)
			{
				TriggerReload();
			}

			if (base.characterBody)
            {
				base.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, 1f);
            }

			//Spotter linger runs on all players.
			SpotterTargetingController stc = base.GetComponent<SpotterTargetingController>();
			if (stc && stc.spotterFollower)
			{
				stc.spotterFollower.SetLinger(base.characterBody.corePosition, 2f);
			}
		}

		private void RecalculateRollSpeed()
		{
			this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(CombatRoll.initialSpeedCoefficient, CombatRoll.finalSpeedCoefficient, base.fixedAge / CombatRoll.duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.RecalculateRollSpeed();

            //base.characterDirection.forward = this.forwardDirection;

			if (base.cameraTargetParams && (!scopeController || !scopeController.IsScoped))
			{
				base.cameraTargetParams.fovOverride = Mathf.Lerp(CombatRoll.dodgeFOV, 60f, base.fixedAge / CombatRoll.duration);
			}
			Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
			if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
			{
				Vector3 vector = normalized * this.rollSpeed;
				float y = vector.y;
				vector.y = 0f;
				float d = Mathf.Max(Vector3.Dot(vector, this.forwardDirection), 0f);
				vector = this.forwardDirection * d;
				vector.y += Mathf.Max(y, 0f);
				base.characterMotor.velocity = vector;
			}
			this.previousPosition = base.transform.position;
			if (base.fixedAge >= CombatRoll.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

        public override void Update()
        {
            base.Update();
            if (fixedAge < 0.75f * duration)
            {
                base.characterDirection.forward = this.forwardDirection;
            }
        }

        public override void OnExit()
		{
			if (base.cameraTargetParams && (!scopeController || !scopeController.IsScoped))
			{
				base.cameraTargetParams.fovOverride = -1f;
			}

			base.OnExit();
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(this.forwardDirection);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			this.forwardDirection = reader.ReadVector3();
		}

		private void TriggerReload()
        {
			ReloadController rc = base.characterBody.GetComponent<ReloadController>();
			if (rc)
            {
				rc.AutoReload();
            }
		}

		public static float duration = 0.4f;
		public static float initialSpeedCoefficient = 5f * 1.45f;
		public static float finalSpeedCoefficient = 2.5f * 1.45f;

		public static string dodgeSoundString = EntityStates.Commando.DodgeState.dodgeSoundString;
		public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

		public static float initialHopVelocity = 20f;

		private float rollSpeed;
		private Vector3 forwardDirection;
		private Animator animator;
		private Vector3 previousPosition;
		private SniperClassic.ScopeController scopeController;
	}
}