using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.SniperClassicSkills
{
    class SecondaryScope : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();

			scopeComponent = base.gameObject.GetComponent<SniperClassic.ScopeController>();
			if (scopeComponent)
            {
				scopeComponent.EnterScope();
            }

			if (NetworkServer.active && base.characterBody)
			{
				base.characterBody.AddBuff(BuffIndex.Slow50);
			}
			if (base.cameraTargetParams)
			{
				base.cameraTargetParams.aimMode = CameraTargetParams.AimType.FirstPerson;
				base.cameraTargetParams.fovOverride = 40f;
			}
			if (base.characterBody)
			{
				this.originalCrosshairPrefab = base.characterBody.crosshairPrefab;
				base.characterBody.crosshairPrefab = SecondaryScope.crosshairPrefab;
			}
			this.laserPointerObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/LaserPointerBeamEnd"));
			this.laserPointerObject.GetComponent<LaserPointerController>().source = base.inputBank;
		}
		public override void OnExit()
		{
			EntityState.Destroy(this.laserPointerObject);
			if (NetworkServer.active && base.characterBody)
			{
				base.characterBody.RemoveBuff(BuffIndex.Slow50);
			}
			if (base.cameraTargetParams)
			{
				base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
				base.cameraTargetParams.fovOverride = -1f;
			}
			if (base.characterBody)
			{
				base.characterBody.crosshairPrefab = this.originalCrosshairPrefab;
			}
			if (scopeComponent)
			{
				scopeComponent.ExitScope();
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.isSprinting = false;
			if (base.cameraTargetParams)
			{
				base.cameraTargetParams.fovOverride = 30f;
			}
			if (scopeComponent)
			{
				scopeComponent.AddCharge(Time.fixedDeltaTime * this.attackSpeedStat / SecondaryScope.baseChargeDuration);
			}
			if (base.isAuthority && (!base.inputBank || !base.inputBank.skill2.down))
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		public static float baseChargeDuration = 3f;
		public static GameObject crosshairPrefab;

		private GameObject originalCrosshairPrefab;
		private GameObject laserPointerObject;
		private SniperClassic.ScopeController scopeComponent;
	}
}
