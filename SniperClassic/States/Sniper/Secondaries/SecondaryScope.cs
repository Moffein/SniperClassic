using BepInEx.Configuration;
using EntityStates;
using RoR2;
using RoR2.UI;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.CameraTargetParams;

namespace EntityStates.SniperClassicSkills
{
	public class SecondaryScope : BaseState
	{
		private CameraParamsOverrideHandle camOverrideHandle;

		private CharacterCameraParamsData shoulderCameraParams = new CharacterCameraParamsData()
		{
			isFirstPerson = false,
			maxPitch = 70,
			minPitch = -70,
			pivotVerticalOffset = -0f,
			idealLocalCameraPos = new Vector3(1.8f, -0.2f, -6f),
			wallCushion = 0.1f,
		};

		private CharacterCameraParamsData scopeCameraParams = new CharacterCameraParamsData()
		{
			isFirstPerson = true,
			maxPitch = 70,
			minPitch = -70,
			pivotVerticalOffset = 0f,
			idealLocalCameraPos = new Vector3(0f, 0f, 0f),	//same as aimorigin
			wallCushion = 0.1f,
		};


		public override void OnEnter()
		{
			base.OnEnter();

			this.chargeDuration = 1.5f;
			if (!(base.characterBody && base.characterBody.master && base.characterBody.master.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0))
			{
				if (base.skillLocator)
				{
					switch (base.skillLocator.primary.baseSkill.skillName)
					{
						case "Snipe":
							this.chargeDuration = Snipe.baseChargeDuration;
							break;
						case "HeavySnipe":
							this.chargeDuration = HeavySnipe.baseChargeDuration;
							break;
						case "FireBR":
							this.chargeDuration = FireBattleRifle.baseChargeDuration;
							break;
						default:
							break;
					}
				}
			}

			base.GetModelAnimator().SetBool("scoped", true);

			currentFOV = zoomFOV.Value;
			scopeComponent = base.gameObject.GetComponent<SniperClassic.ScopeController>();
			if (scopeComponent)
			{
				scopeComponent.EnterScope();
				currentFOV = scopeComponent.storedFOV;
			}

			if (NetworkServer.active && base.characterBody)
			{
				base.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
			}

			if (base.characterBody)
			{
				this.originalCrosshairPrefab = base.characterBody.defaultCrosshairPrefab;
				if (base.cameraTargetParams)
				{
					//this.initialCameraPosition = base.cameraTargetParams.idealLocalCameraPos;
					//this.cameraOffset = SecondaryScope.idealLocalCameraPosition - this.initialCameraPosition;

					GameObject selectedCrosshair = null;
					if (currentFOV == maxFOV)
					{
						//base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
						selectedCrosshair = SecondaryScope.noscopeCrosshairPrefab;
					}
					else
					{
						//base.cameraTargetParams.aimMode = CameraTargetParams.AimType.FirstPerson;
						selectedCrosshair = SecondaryScope.scopeCrosshairPrefab;
					}

					this.crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, selectedCrosshair, CrosshairUtils.OverridePriority.Skill);
					currentCrosshairPrefab = selectedCrosshair;

					CameraParamsOverrideRequest request = new CameraParamsOverrideRequest
					{
						cameraParamsData = currentCrosshairPrefab == SecondaryScope.scopeCrosshairPrefab ? scopeCameraParams : shoulderCameraParams,
						priority = 0,
					};
					request.cameraParamsData.fov = currentFOV;
					camOverrideHandle = base.cameraTargetParams.AddParamsOverride(request, 0.2f);
				}
			}
			this.laserPointerObject = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/LaserPointerBeamEnd"));
			this.laserPointerObject.GetComponent<LaserPointerController>().source = base.inputBank;
		}

		public override void OnExit()
		{
			EntityState.Destroy(this.laserPointerObject);
			if (NetworkServer.active && base.characterBody)
			{
				if (base.characterBody.HasBuff(RoR2Content.Buffs.Slow50))
				{
					base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
				}
			}
			if (base.cameraTargetParams)
			{
				base.cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);
			}

			CrosshairUtils.OverrideRequest overrideRequest = this.crosshairOverrideRequest;
			if (overrideRequest != null)
			{
				overrideRequest.Dispose();
			}
			base.GetModelAnimator().SetBool("scoped", false);
			if (scopeComponent)
			{
				scopeComponent.SetStoredFoV(currentFOV);
				scopeComponent.ExitScope();

				if (heavySlow)
				{
					scopeComponent.ResetCharge();
				}
			}

			//Leftover from when Heavy Snipe restricted jumping.
			if (base.characterMotor && base.characterMotor.isGrounded)
			{
				base.characterMotor.jumpCount = 0;
			}
			base.OnExit();
		}

        public override void Update() {
            base.Update();

			float startFOV = currentFOV;

			bool cameraTogglePressed = SniperClassic.Util.GetKeyPressed(cameraToggleKey);
			if (!cameraToggleWasPressed && cameraTogglePressed)
			{
				if (currentFOV >= maxFOV)
				{
					currentFOV = zoomFOV.Value;
				}
				else
				{
					currentFOV = maxFOV;
				}
			}
			cameraToggleWasPressed = cameraTogglePressed;

			if (currentFOV < minFOV) {
				currentFOV = minFOV;
			} else if (currentFOV > maxFOV) {
				currentFOV = maxFOV;
			}

			if (startFOV != currentFOV) {
				fovChanged = true;
			}
		}

        public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.StartAimMode();

			if (heavySlow && base.characterMotor && base.characterBody)
			{
				base.characterMotor.jumpCount = base.characterBody.maxJumpCount;
			}

			if (!buttonReleased && base.inputBank && !base.inputBank.skill2.down)
			{
				buttonReleased = true;
			}

			if (base.isAuthority)
			{
				bool shouldExit = false;
				if (base.inputBank)
				{
					shouldExit = !base.inputBank.skill2.down && (!SecondaryScope.toggleScope.Value || beginExit);
					if ((base.inputBank.skill2.down && SecondaryScope.toggleScope.Value && buttonReleased))
					{
						beginExit = true;	//For some reason MustKeyPress on the SkillDef isn't working.
					}
				}

				if (!base.inputBank || shouldExit)
				{
					this.outer.SetNextStateToMain();
					return;
				}
			}

			base.characterBody.isSprinting = false;
			if (scopeComponent)
			{
				scopeComponent.SetStoredFoV(currentFOV);
				scopeComponent.AddCharge(Time.fixedDeltaTime * this.attackSpeedStat / this.chargeDuration);
			}

			UpdateCrosshairAndCamera();
		}

		private void UpdateCrosshairAndCamera()
		{
			if (base.characterBody && base.cameraTargetParams)
			{
				GameObject newCrosshairPrefab = currentCrosshairPrefab;
				if (currentFOV == maxFOV)
				{
					newCrosshairPrefab = SecondaryScope.noscopeCrosshairPrefab;
				}
				else
				{
					newCrosshairPrefab = SecondaryScope.scopeCrosshairPrefab;
				}
				if (currentCrosshairPrefab != newCrosshairPrefab)
				{
					CrosshairUtils.OverrideRequest overrideRequest = this.crosshairOverrideRequest;
					if (overrideRequest != null)
					{
						overrideRequest.Dispose();
					}
					this.crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, newCrosshairPrefab, CrosshairUtils.OverridePriority.Skill);
					currentCrosshairPrefab = newCrosshairPrefab;
				}
				if (fovChanged)
                {
					fovChanged = false;
					if (base.cameraTargetParams)
					{
						base.cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);
						CameraParamsOverrideRequest request = new CameraParamsOverrideRequest
						{
							cameraParamsData = currentCrosshairPrefab == SecondaryScope.scopeCrosshairPrefab ? scopeCameraParams : shoulderCameraParams,
							priority = 0
						};
						request.cameraParamsData.fov = currentFOV;
						camOverrideHandle = base.cameraTargetParams.AddParamsOverride(request, 0.2f);
					}
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		public static ConfigEntry<KeyboardShortcut> cameraToggleKey;
		public static float maxFOV = 50f;
		public static float minFOV = 5f;
		public static ConfigEntry<float> zoomFOV;
		public static GameObject scopeCrosshairPrefab;
		public static GameObject noscopeCrosshairPrefab;
		public static bool resetZoom = true;
		public static ConfigEntry<bool> toggleScope;

		private float currentFOV = 40f;
		private GameObject originalCrosshairPrefab;
		private GameObject laserPointerObject;
		public ScopeController scopeComponent;
		private bool buttonReleased = false;
		private float chargeDuration;
		private Vector3 cameraOffset;
		private Vector3 initialCameraPosition;
		private bool heavySlow = false;
		private bool cameraToggleWasPressed = false;
		private bool beginExit = false;

		private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
		private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;
		private GameObject currentCrosshairPrefab;

		private bool fovChanged = false;
	}
}