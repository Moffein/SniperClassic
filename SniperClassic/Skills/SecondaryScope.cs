using EntityStates;
using RoR2;
using SniperClassic;
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

			this.initialTime = Time.fixedTime;

			if (base.skillLocator)
            {
				if (base.skillLocator.primary.skillDef.skillName == "Snipe")
                {
					this.chargeDuration = Snipe.baseChargeDuration;
                }
				else if (base.skillLocator.primary.skillDef.skillName == "FireBR")
                {
					this.chargeDuration = FireBattleRifle.baseChargeDuration;
                }
				else
                {
					this.chargeDuration = 1f;
				}
            }

			currentFOV = zoomFOV;
			scopeComponent = base.gameObject.GetComponent<SniperClassic.ScopeController>();
			if (scopeComponent)
            {
				scopeComponent.EnterScope();
				if (!resetZoom)
                {
					currentFOV = scopeComponent.storedFOV;
                }
            }

			if (NetworkServer.active && base.characterBody)
			{
				base.characterBody.AddBuff(BuffIndex.Slow50);
			}
			if (base.characterBody)
			{
				this.originalCrosshairPrefab = base.characterBody.crosshairPrefab;
				if (base.cameraTargetParams)
				{
					base.cameraTargetParams.fovOverride = currentFOV;

					if (currentFOV == maxFOV)
					{
						base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
						base.characterBody.crosshairPrefab = SecondaryScope.noscopeCrosshairPrefab;
					}
					else
					{
						base.cameraTargetParams.aimMode = CameraTargetParams.AimType.FirstPerson;
						base.characterBody.crosshairPrefab = SecondaryScope.scopeCrosshairPrefab;
					}
				}
				
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
				scopeComponent.storedFOV = currentFOV;
				scopeComponent.ExitScope();
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.StartAimMode();
			if (!buttonReleased && base.inputBank && !base.inputBank.skill2.down)
            {
				buttonReleased = true;
            }

			if (base.isAuthority && (!base.inputBank || (!base.inputBank.skill2.down && !toggleScope) || (base.inputBank.skill2.down && toggleScope && buttonReleased)))
			{
				this.outer.SetNextStateToMain();
				return;
			}

			if (useScrollWheelZoom)
            {
				float scrollMovement = Input.GetAxis("Mouse ScrollWheel") * SecondaryScope.scrollZoomSpeed;
				if (!invertScrollWheelZoom)
                {
					scrollMovement *= -1f;
                }
				currentFOV += scrollMovement;
			}

			if (Input.GetKey(zoomInKey))
			{
				currentFOV -= buttonZoomSpeed;
			}
			if (Input.GetKey(zoomOutKey))
			{
				currentFOV += buttonZoomSpeed;
			}

			if (currentFOV < minFOV)
            {
				currentFOV = minFOV;
            }
			else if (currentFOV > maxFOV)
            {
				currentFOV = maxFOV;
            }

			base.characterBody.isSprinting = false;
			if (base.characterBody && base.cameraTargetParams)
			{
				base.cameraTargetParams.fovOverride = currentFOV;
				if (currentFOV == maxFOV)
				{
					base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
					base.characterBody.crosshairPrefab = SecondaryScope.noscopeCrosshairPrefab;

					//Ripped straight from Enforcer
					float denom = (1 + Time.fixedTime - this.initialTime);
					float smoothFactor = 8 / Mathf.Pow(denom, 2);
					Vector3 smoothVector = new Vector3(-3 / 20, 1 / 16, -1);
					base.cameraTargetParams.idealLocalCameraPos = new Vector3(1.8f, -0.5f, -6f) + smoothFactor * smoothVector;
				}
				else
				{
					base.cameraTargetParams.aimMode = CameraTargetParams.AimType.FirstPerson;
					base.characterBody.crosshairPrefab = SecondaryScope.scopeCrosshairPrefab;
				}
			}
			if (scopeComponent)
			{
				scopeComponent.storedFOV = currentFOV;
				scopeComponent.AddCharge(Time.fixedDeltaTime * this.attackSpeedStat / this.chargeDuration);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		public static float maxFOV = 50f;
		public static float minFOV = 5f;
		public static float zoomFOV = 50f;
		public static float scrollZoomSpeed = 20f;
		public static float buttonZoomSpeed = 1f;
		public static GameObject scopeCrosshairPrefab;
		public static GameObject noscopeCrosshairPrefab;
		public static bool resetZoom = true;
		public static bool toggleScope = true;

		public static bool useScrollWheelZoom = true;
		public static bool invertScrollWheelZoom = false;
		public static KeyCode zoomInKey = KeyCode.None;
		public static KeyCode zoomOutKey = KeyCode.None;

		private float currentFOV = 40f;
		private GameObject originalCrosshairPrefab;
		private GameObject laserPointerObject;
		public SniperClassic.ScopeController scopeComponent;
		private bool buttonReleased = false;
		private float chargeDuration;
		private float initialTime;
	}
}
