using EntityStates.Missions.Arena.NullWard;
using EntityStates.SniperClassicSkills;
using R2API;
using RewiredConsts;
using RoR2;
using SniperClassic.Controllers;
using SniperClassic.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
	public class SpotterFollowerController : NetworkBehaviour
	{
		private void FixedUpdate()
		{
			if (this.cachedTargetMasterNetID != this.__targetMasterNetID)
			{
				this.cachedTargetBodyObject = (this.__targetMasterNetID != __ownerMasterNetID) ? FindBodyOnClient(__targetMasterNetID) : OwnerBodyObject;
				this.targetBodyObject = this.cachedTargetBodyObject;
				this.cachedTargetMasterNetID = this.__targetMasterNetID;
				this.OnTargetChanged();
			}
			if (NetworkServer.active)
			{
				this.FixedUpdateServer();
			}

			if (lingerTimer > 0f)
            {
				lingerTimer -= Time.fixedDeltaTime;

				if (__targetingEnemy)
                {
					lingerTimer = 0f;
                }
            }
		}

		public bool IsTargetingEnemy()
        {
			return __targetingEnemy;
        }

		private void OnDestroy()
		{
			if (NetworkServer.active)
			{
				if (this.cachedTargetBody)
				{
					ClearBuffs(cachedTargetBody);
				}
				if (currentDisruptTarget)
				{
					Destroy(currentDisruptTarget);
				}
			}
		}

		private void ClearBuffs(CharacterBody body)
		{
			if (body.HasBuff(SniperContent.spotterBuff))
			{
				body.RemoveBuff(SniperContent.spotterBuff);
			}
			if (body.HasBuff(SniperContent.spotterScepterBuff))
			{
				body.RemoveBuff(SniperContent.spotterScepterBuff);
			}
			if (body.HasBuff(SniperContent.spotterStatDebuff))
			{
				body.RemoveBuff(SniperContent.spotterStatDebuff);
			}
			while (body.HasBuff(SniperContent.spotterCooldownBuff))
			{
				body.RemoveBuff(SniperContent.spotterCooldownBuff);
			}
		}


		[Server]
		public void __AssignNewTarget(uint netID)
		{
			if (!NetworkServer.active)
			{
				return;
			}

			if (this.cachedTargetBody && this.cachedTargetBody != ownerBody)
			{
				ClearBuffs(cachedTargetBody);
			}

			disruptActive = false;
			Destroy(currentDisruptTarget);

			GameObject target = FindBodyOnClient(netID);
			__targetMasterNetID = (target ? netID : __ownerMasterNetID);
			this.targetBodyObject = (target ? target : this.OwnerBodyObject);
			this.cachedTargetBodyObject = this.targetBodyObject;

			if (__targetMasterNetID != __ownerMasterNetID)
			{
				__targetingEnemy = true;
			}
			else
			{
				__targetingEnemy = false;
			}

			this.OnTargetChanged();

			ApplyDebuff();

			if (__targetingEnemy)
			{
				if (spotterMode == SpotterMode.Disrupt || spotterMode == SpotterMode.DisruptScepter)
				{
					disruptActive = true;
					currentDisruptTarget = cachedTargetBodyObject.AddComponent<EnemyDisruptComponent>();
					if (currentDisruptTarget)
					{
						currentDisruptTarget.scepter = spotterMode == SpotterMode.DisruptScepter;
						currentDisruptTarget.attacker = OwnerBodyObject;
						currentDisruptTarget.attackerBody = ownerBody;
						currentDisruptTarget.teamIndex = ownerBody.teamComponent.teamIndex;

						currentDisruptTarget.victimBody = cachedTargetBody;
						currentDisruptTarget.victimTeamIndex = cachedTargetBody.teamComponent.teamIndex;

						currentDisruptTarget.scaledHitDelay = EnemyDisruptComponent.baseHitDelay * ownerBody.attackSpeed;
						currentDisruptTarget.scaledHitCount = EnemyDisruptComponent.baseHitCount * ownerBody.attackSpeed;
					}
				}
			}
		}

		private void OnTargetChanged()
		{
			this.cachedTargetBody = (this.cachedTargetBodyObject ? this.cachedTargetBodyObject.GetComponent<CharacterBody>() : null);

			while (spotterTargetHighlights.Count > 0)
			{
				if (spotterTargetHighlights[0] && spotterTargetHighlights[0].gameObject) UnityEngine.Object.Destroy(spotterTargetHighlights[0].gameObject);
				spotterTargetHighlights.RemoveAt(0);
			}
			if (this.cachedTargetBodyObject && this.cachedTargetBodyObject != this.ownerBodyObject)
			{
				spotterTargetHighlights = spotterTargetHighlights.Concat(SpotterTargetHighlight.Create(this.cachedTargetBody, TeamComponent.GetObjectTeam(this.OwnerBodyObject))).ToList();
			}

			if (NetworkServer.active)
			{
				if (enemySpotterReference)
				{
					Destroy(enemySpotterReference);
					enemySpotterReference = null;
				}

				if (this.cachedTargetBodyObject && this.cachedTargetBodyObject != this.ownerBodyObject)
				{
					enemySpotterReference = this.cachedTargetBodyObject.AddComponent<EnemySpotterReference>();
					enemySpotterReference.spotterOwner = this.ownerBodyObject;
				}
			}
		}

		private void FixedUpdateServer()
		{
			if (this.cachedTargetBodyObject != this.targetBodyObject)
			{
				this.cachedTargetBodyObject = this.targetBodyObject;
				this.OnTargetChanged();
			}

			if (!this.targetBodyObject)
			{
				this.targetBodyObject = this.OwnerBodyObject;
				__targetMasterNetID = __ownerMasterNetID;
			}
			if (!this.OwnerBodyObject)
			{
				this.OwnerBodyObject = FindBodyOnClient(__ownerMasterNetID);
				if (!this.OwnerBodyObject)
				{
					if (this.cachedTargetBody)
					{
						ClearBuffs(this.cachedTargetBody);
					}
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			ApplyDebuff();
			CheckDisrupt();
		}

		private GameObject FindBodyOnClient(uint masterID)
		{
			if (masterID == __ownerMasterNetID)
			{
				return OwnerBodyObject;
			}

			GameObject find = ClientScene.FindLocalObject(new NetworkInstanceId(masterID));
			if (find)
			{
				CharacterMaster cm = find.GetComponent<CharacterMaster>();
				if (cm)
				{
					return cm.GetBodyObject();
				}
			}
			return null;
		}

		private void Update()
		{
			this.UpdateMotion();
			base.transform.position += this.velocity * Time.deltaTime;

			if (__targetingEnemy)
			{
				base.transform.rotation = Quaternion.AngleAxis(this.rotationAngularVelocity * Time.deltaTime, Vector3.up) * base.transform.rotation;
			}

			if (__targetingEnemy != cachedTargetingEnemy)
			{
				cachedTargetingEnemy = __targetingEnemy;
				if (cachedTargetingEnemy)
				{
					base.transform.localScale = enemyScale;
				}
				else
				{
					base.transform.localScale = playerScale;
				}
			}
		}

		[Server]
		private void CheckDisrupt()
		{
			if (spotterMode == SpotterMode.Disrupt || spotterMode == SpotterMode.DisruptScepter)
			{
				if (disruptActive)
				{
					if (!currentDisruptTarget)
					{
						currentDisruptProgress = EnemyDisruptComponent.baseHitCount;
						targetingController.ServerForceEndSpotterSkill();
					}
					else
					{
						currentDisruptProgress = currentDisruptTarget.hitCounter;
					}
				}
			}
		}


		[Server]
		private void ApplyDebuff()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			if (!this.cachedTargetBody || this.cachedTargetBodyObject == this.OwnerBodyObject)
			{
				return;
			}
			if (!this.cachedTargetBody.HasBuff(SniperContent.spotterStatDebuff))
			{
				this.cachedTargetBody.AddBuff(SniperContent.spotterStatDebuff);
			}

			BuffIndex buffToCheck = BuffIndex.None;
			switch (spotterMode)
			{
				case SpotterMode.ChainLightningScepter:
					buffToCheck = SniperContent.spotterScepterBuff.buffIndex;
					break;
				case SpotterMode.ChainLightning:
					buffToCheck = SniperContent.spotterBuff.buffIndex;
					break;
				default:
					break;
			}

			if (buffToCheck != BuffIndex.None)
			{
				bool shouldApplyBuff = rechargeController.SpotterReady();
				if (!shouldApplyBuff)
				{
					if (this.cachedTargetBody.HasBuff(buffToCheck))
					{
						this.cachedTargetBody.RemoveBuff(buffToCheck);
					}

					int cooldownCount = this.cachedTargetBody.GetBuffCount(SniperContent.spotterCooldownBuff.buffIndex);
					int desiredCooldown = rechargeController.GetCooldown();

					while (cooldownCount > desiredCooldown)
					{
						cooldownCount--;
						this.cachedTargetBody.RemoveBuff(SniperContent.spotterCooldownBuff.buffIndex);
					}
					
					while (cooldownCount < desiredCooldown)
					{
						cooldownCount++;
						this.cachedTargetBody.AddBuff(SniperContent.spotterCooldownBuff.buffIndex);
					}
				}
				else
				{
					if (!this.cachedTargetBody.HasBuff(buffToCheck))
					{
						this.cachedTargetBody.AddBuff(buffToCheck);
					}
					while (this.cachedTargetBody.HasBuff(SniperContent.spotterCooldownBuff.buffIndex))
					{
						this.cachedTargetBody.RemoveBuff(SniperContent.spotterCooldownBuff.buffIndex);
					}
				}
			}
		}


		public override void OnStartClient()
		{
			base.OnStartClient();
			base.transform.position = this.GetTargetPosition();
			base.transform.localScale = playerScale;
			OwnerBodyObject = FindBodyOnClient(__ownerMasterNetID);
		}

		private Vector3 GetTargetPosition()
		{
			if (!__targetingEnemy && lingerTimer > 0f)
            {
				return lingerPosition;
            }

			GameObject gameObject = this.targetBodyObject ?? this.OwnerBodyObject;
			if (!gameObject)
			{
				return base.transform.position;
			}
			CharacterBody component = gameObject.GetComponent<CharacterBody>();
			if (!component)
			{
				return gameObject.transform.position;
			}
			return component.corePosition;
		}

		private void UpdateMotion()
		{
			Vector3 offset = enemyOffset;
			if (!__targetingEnemy && ownerBody && ownerBody.inputBank)
			{
				offset = ownerBody.inputBank.aimDirection;
				offset.y = 0;
				offset.Normalize();
				offset = Quaternion.AngleAxis(90f, Vector3.up) * offset * -1.8f;
				offset.y = 1.5f;

				if (ownerBody.modelLocator && ownerBody.modelLocator.modelTransform)
				{
					base.transform.rotation = ownerBody.modelLocator.modelTransform.rotation;
				}
			}
			Vector3 desiredPosition = this.GetTargetPosition() + offset;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, desiredPosition, ref this.velocity, this.damping);
		}

		public void setModelSkin(CharacterModel model)
		{
			GetComponentInChildren<Renderer>().material = model.baseRendererInfos[2].defaultMaterial;
			GetComponentInChildren<MeshFilter>().mesh = model.baseRendererInfos[2].renderer.GetComponent<MeshFilter>().mesh;
		}

		//public static GameObject disruptEffectPrefab = Resources.Load<GameObject>("prefabs/effects/smokescreeneffect");
		public bool disruptActive = false;
		public EnemyDisruptComponent currentDisruptTarget = null;
		[SyncVar]
		public int currentDisruptProgress = 0;

		public float rotationAngularVelocity = 40f;
		public float acceleration = 20f;
		public float damping = 0.1f;

		public CharacterBody ownerBody;
		private GameObject ownerBodyObject;

		//Linger behavior is clientside
		private float lingerTimer = 0f;
		private Vector3 lingerPosition = Vector3.zero;
		public void SetLinger(Vector3 position, float lingerTime)
        {
			if (!__targetingEnemy)
			{
				lingerPosition = position;
				lingerTimer = lingerTime;
			}
        }

		public GameObject OwnerBodyObject
		{
			get => ownerBodyObject;
			set
			{
				ownerBodyObject = value;
				if (ownerBodyObject != null)
				{
					setModelSkin(ownerBodyObject.GetComponent<CharacterBody>().modelLocator.modelTransform.GetComponent<CharacterModel>());
				}
			}
		}

		public GameObject targetBodyObject;

		public SpotterTargetingController targetingController;
		public SpotterRechargeController rechargeController;

		public SpotterMode spotterMode = SpotterMode.ChainLightning;

		[SyncVar]
		public uint __ownerMasterNetID = uint.MaxValue; //trying to find body on client with this doesn't work
		[SyncVar]
		public uint __targetMasterNetID = uint.MaxValue;

		public uint cachedTargetMasterNetID = uint.MaxValue;

		private GameObject cachedTargetBodyObject;
		private CharacterBody cachedTargetBody;
		private Vector3 velocity = Vector3.zero;

		[SyncVar]
		private bool __targetingEnemy = false;

		private bool cachedTargetingEnemy = false;

		public bool setOwner = false;

		private Vector3 enemyOffset = new Vector3(0, 5.5f, 0);
		private Vector3 enemyScale = new Vector3(2, 2, 2);
		private Vector3 playerScale = new Vector3(1, 1, 1);

		private EnemySpotterReference enemySpotterReference = null;

		private List<SpotterTargetHighlight> spotterTargetHighlights = new List<SpotterTargetHighlight>();

		public static GameObject spotterTargetHighlightPrefab;

		public class SpotterTargetHighlight : MonoBehaviour
        {
            public CharacterBody targetBody;
            public TextMeshProUGUI textTargetName;
            public TextMeshProUGUI textTargetHP;
            public Canvas canvas;
            public Camera uiCam;
            public Camera sceneCam;
            public float timeScan = 0f;
            public float timeScanMax = 0.25f;	//default 0.5
            public float timeWrite = 0f;
            public float timeWriteMax = 0.25f;  //default 0.5
			public float[] scans;
            public int scanPosition = 0;
            public GameObject insideViewObject;
            public GameObject outsideViewObject;

            public static List<SpotterTargetHighlight> Create(CharacterBody targetBody, TeamIndex teamIndex)
            {
                List<SpotterTargetHighlight> components = new List<SpotterTargetHighlight>();
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (TeamComponent.GetObjectTeam(cameraRigController.targetBody.gameObject) == teamIndex)
                    {
                        SpotterTargetHighlight component = UnityEngine.Object.Instantiate<GameObject>(SpotterFollowerController.spotterTargetHighlightPrefab).GetComponent<SpotterTargetHighlight>();
                        component.targetBody = targetBody;
                        component.canvas.worldCamera = cameraRigController.uiCam;
                        component.uiCam = cameraRigController.uiCam;
                        component.sceneCam = cameraRigController.sceneCam;
                        components.Add(component);
                    }
                }
                return components;
            }

            public void Awake()
            {
                canvas = GetComponent<Canvas>();
                scans = new float[2];

				if (textTargetName) textTargetName.font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
				if (textTargetHP) textTargetHP.font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            }

            public void OnEnable()
            {
                instances.Add(this);
            }

            public void OnDisable()
            {
                instances.Remove(this);
            }

            public static void UpdateAll()
            {
                for (int i = instances.Count - 1; i >= 0; i--) instances[i].DoUpdate();
            }

            public static List<SpotterTargetHighlight> instances = new List<SpotterTargetHighlight>();

            public void DoUpdate()
            {
                if (!targetBody)
                {
					UnityEngine.Object.Destroy(gameObject);
                    return;
                }
                Vector3 screenPoint = sceneCam.WorldToScreenPoint(targetBody.corePosition);
                bool targetBehindCamera = screenPoint.z <= 0f;
                bool targetInsideView = !targetBehindCamera && sceneCam.pixelRect.Contains(new Vector2(screenPoint.x, screenPoint.y));
                if (insideViewObject)
                {
                    insideViewObject.transform.position = screenPoint;
                    insideViewObject.SetActive(RoR2.UI.HUD.cvHudEnable.value && targetInsideView);
                }
                if (outsideViewObject)
                {
                    Vector2 screenCenter = new Vector2(sceneCam.pixelWidth * 0.5f, sceneCam.pixelHeight * 0.5f);
                    Vector2 centerOffset = (new Vector2(screenPoint.x, screenPoint.y) - screenCenter) * (targetBehindCamera ? -1f : 1f);
                    Vector2 outsideViewScreenPoint = screenCenter + centerOffset / Mathf.Max(
                        Mathf.Abs(centerOffset.x / (sceneCam.pixelWidth * 0.5f)),
                        Mathf.Abs(centerOffset.y / (sceneCam.pixelHeight * 0.5f))
                    );

                    outsideViewObject.transform.position = new Vector3(outsideViewScreenPoint.x, outsideViewScreenPoint.y, 1f);
                    outsideViewObject.transform.localEulerAngles = new Vector3(0f, 0f,
                        Vector2.SignedAngle(Vector2.up, -centerOffset)
                    );
                    outsideViewObject.SetActive(RoR2.UI.HUD.cvHudEnable.value && !targetInsideView);
                }
                if (scanPosition < scans.Length)
                {
                    if (timeScan < timeScanMax) timeScan += Time.deltaTime;
                    else
                    {
                        if (timeWrite < timeWriteMax)
                        {
                            timeWrite += Time.deltaTime;
                            scans[scanPosition] = timeWrite / timeWriteMax;
                            if (timeWrite >= timeWriteMax)
                            {
                                timeScan = 0f;
                                timeWrite = 0f;
                                scanPosition++;
                            }
                        }
                    }
                }
                string bodyName = Util.GetBestBodyName(targetBody.gameObject);
                textTargetName.text = scans[0] < 1f ? bodyName.Remove(Mathf.FloorToInt(bodyName.Length * scans[0]), Mathf.FloorToInt(bodyName.Length * (1f - scans[0]))) + "_" : bodyName;
                HealthComponent healthComponent = targetBody.healthComponent;
                if (healthComponent)
                {
                    string healthString = string.Format("{0}/{1}", Mathf.Ceil(healthComponent.combinedHealth), Mathf.Ceil(healthComponent.fullHealth));
                    textTargetHP.text = scans[1] < 1f ? healthString.Remove(Mathf.FloorToInt(healthString.Length * scans[1]), Mathf.FloorToInt(healthString.Length * (1f - scans[1]))) + "_" : healthString;
                }

				if (!Modules.Config.spotterUI)
				{
					textTargetName.alpha = 0f;
					textTargetHP.alpha = 0f;
				}
			}
        }
	}
}