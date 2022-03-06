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
	public class SecondaryScope : BaseState
	{
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

			currentFOV = zoomFOV;
			scopeComponent = base.gameObject.GetComponent<SniperClassic.ScopeController>();
			if (scopeComponent)
			{
				scopeComponent.EnterScope();
				if (!csgoZoom && !resetZoom)
				{
					currentFOV = scopeComponent.storedFOV;
				}
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
					this.initialCameraPosition = base.cameraTargetParams.idealLocalCameraPos;
					this.cameraOffset = SecondaryScope.idealLocalCameraPosition - this.initialCameraPosition;

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
				base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
				base.cameraTargetParams.fovOverride = -1f;
			}
			if (base.characterBody)
			{
				base.characterBody.crosshairPrefab = this.originalCrosshairPrefab;
			}
			base.GetModelAnimator().SetBool("scoped", false);
			if (scopeComponent)
			{
				scopeComponent.storedFOV = currentFOV;
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
				if (!csgoZoom)
				{
					if ((!base.inputBank || (!base.inputBank.skill2.down && !toggleScope) || (base.inputBank.skill2.down && toggleScope && buttonReleased)))
					{
						this.outer.SetNextStateToMain();
						return;
					}
				}
				else
				{
					if (csgoZoomStopwatch < csgoZoomCooldown)
					{
						csgoZoomStopwatch += Time.fixedDeltaTime;
					}
					else if (base.inputBank.skill2.down && buttonReleased)
					{
						csgoZoomStopwatch = 0f;
						csgoZoomCount++;
						float newFov = 0f;
						switch (csgoZoomCount)
						{
							case 0:
								newFov = maxFOV;
								break;
							case 1:
								newFov = minFOV + (maxFOV - minFOV) / 2f;
								break;
							case 2:
								newFov = minFOV;
								break;
							default:
								this.outer.SetNextStateToMain();
								return;
						}
						currentFOV = newFov;
					}
				}
			}

			if (!csgoZoom)
			{
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
			if (scopeComponent)
			{
				scopeComponent.storedFOV = currentFOV;
				scopeComponent.AddCharge(Time.fixedDeltaTime * this.attackSpeedStat / this.chargeDuration);
			}
		}

		public override void Update()
		{
			base.Update();
			if (base.characterBody && base.cameraTargetParams)
			{
				base.cameraTargetParams.fovOverride = currentFOV;
				if (currentFOV == maxFOV)
				{
					base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
					base.characterBody.crosshairPrefab = SecondaryScope.noscopeCrosshairPrefab;

					float scopePercent = Mathf.Min(SecondaryScope.cameraAdjustTime, base.fixedAge) / SecondaryScope.cameraAdjustTime;

					base.cameraTargetParams.idealLocalCameraPos = this.initialCameraPosition + scopePercent * cameraOffset;
				}
				else
				{
					if (base.cameraTargetParams.aimRequestStack.Count > 0)
					{
						base.cameraTargetParams.aimRequestStack.Clear();
					}
					base.cameraTargetParams.aimMode = CameraTargetParams.AimType.FirstPerson;
					base.characterBody.crosshairPrefab = SecondaryScope.scopeCrosshairPrefab;
				}
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
		public static bool csgoZoom = false;
		private int csgoZoomCount = 0;
		private float csgoZoomCooldown = 0.25f;
		private float csgoZoomStopwatch = 0f;

		public static Vector3 idealLocalCameraPosition = new Vector3(1.8f, -0.5f, -6f);
		public static float cameraAdjustTime = 0.25f;

		public static bool useScrollWheelZoom = true;
		public static bool invertScrollWheelZoom = false;
		public static KeyCode zoomInKey = KeyCode.None;
		public static KeyCode zoomOutKey = KeyCode.None;

		private float currentFOV = 40f;
		private GameObject originalCrosshairPrefab;
		private GameObject laserPointerObject;
		public ScopeController scopeComponent;
		private bool buttonReleased = false;
		private float chargeDuration;
		private Vector3 cameraOffset;
		private Vector3 initialCameraPosition;
		private bool heavySlow = false;
	}
}