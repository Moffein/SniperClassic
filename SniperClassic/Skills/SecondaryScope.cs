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

			currentFOV = zoomFOV;

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
				base.cameraTargetParams.fovOverride = currentFOV;
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

			if (base.isAuthority && (!base.inputBank || !base.inputBank.skill2.down))
			{
				this.outer.SetNextStateToMain();
				return;
			}

			float scrollMovement = -1f * Input.GetAxis("Mouse ScrollWheel") * SecondaryScope.zoomSpeed;
			currentFOV += scrollMovement;
			if (currentFOV < minFOV)
            {
				currentFOV = minFOV;
            }
			else if (currentFOV > maxFOV)
            {
				currentFOV = maxFOV;
            }

			base.characterBody.isSprinting = false;
			if (base.cameraTargetParams)
			{
				base.cameraTargetParams.fovOverride = currentFOV;
			}
			if (scopeComponent)
			{
				scopeComponent.AddCharge(Time.fixedDeltaTime * this.attackSpeedStat / SecondaryScope.baseChargeDuration);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		public static float maxFOV = 80f;
		public static float minFOV = 5f;
		public static float zoomFOV = 80f;
		public static float zoomSpeed = 15f;
		public static float baseChargeDuration = 3f;
		public static GameObject crosshairPrefab;

		private float currentFOV = 40f;
		private GameObject originalCrosshairPrefab;
		private GameObject laserPointerObject;
		private SniperClassic.ScopeController scopeComponent;
	}
}
