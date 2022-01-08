using EntityStates.Missions.Arena.NullWard;
using EntityStates.SniperClassicSkills;
using R2API;
using RewiredConsts;
using RoR2;
using SniperClassic.Controllers;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    class SpotterFollowerController : NetworkBehaviour
    {
		private void FixedUpdate()
		{
			if (this.cachedTargetMasterNetID != this.__targetMasterNetID)
			{
				this.cachedTargetBodyObject = (this.__targetMasterNetID != __ownerMasterNetID) ? FindBodyOnClient(__targetMasterNetID) : ownerBodyObject;
				this.targetBodyObject = this.cachedTargetBodyObject;
				this.cachedTargetMasterNetID = this.__targetMasterNetID;
				this.OnTargetChanged();
			}
			if (NetworkServer.active)
			{
				this.FixedUpdateServer();
			}
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
			this.targetBodyObject = (target ? target : this.ownerBodyObject);
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
						currentDisruptTarget.attacker = ownerBodyObject;
						currentDisruptTarget.attackerBody = ownerBody;
						currentDisruptTarget.teamIndex = ownerBody.teamComponent.teamIndex;

						currentDisruptTarget.victimBody = cachedTargetBody;
						currentDisruptTarget.victimTeamIndex = cachedTargetBody.teamComponent.teamIndex;
					}
				}
			}
		}

		private void OnTargetChanged()
		{
			this.cachedTargetBody = (this.cachedTargetBodyObject ? this.cachedTargetBodyObject.GetComponent<CharacterBody>() : null);
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
				this.targetBodyObject = this.ownerBodyObject;
				__targetMasterNetID = __ownerMasterNetID;
			}
			if (!this.ownerBodyObject)
			{
				this.ownerBodyObject = FindBodyOnClient(__ownerMasterNetID);
				if (!this.ownerBodyObject)
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
				return ownerBodyObject;
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
			if (!this.cachedTargetBody || this.cachedTargetBodyObject == this.ownerBodyObject)
			{
				return;
			}
			if (!this.cachedTargetBody.HasBuff(SniperContent.spotterStatDebuff))
			{
				this.cachedTargetBody.AddBuff(SniperContent.spotterStatDebuff);
			}
			switch (spotterMode)
            {
				case SpotterMode.ChainLightningScepter:
					if (!this.cachedTargetBody.HasBuff(SniperContent.spotterScepterBuff) && !this.cachedTargetBody.HasBuff(SniperContent.spotterCooldownBuff))
					{
						this.cachedTargetBody.AddBuff(SniperContent.spotterScepterBuff);
					}
					break;
				case SpotterMode.ChainLightning:
					if (!this.cachedTargetBody.HasBuff(SniperContent.spotterBuff) && !this.cachedTargetBody.HasBuff(SniperContent.spotterCooldownBuff))
					{
						this.cachedTargetBody.AddBuff(SniperContent.spotterBuff);
					}
					break;
				default:
					break;
			}
		}
		

		public override void OnStartClient()
		{
			base.OnStartClient();
			base.transform.position = this.GetTargetPosition();
			base.transform.localScale = playerScale;
			ownerBodyObject = FindBodyOnClient(__ownerMasterNetID);
		}

		private Vector3 GetTargetPosition()
		{
			GameObject gameObject = this.targetBodyObject ?? this.ownerBodyObject;
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

		//public static GameObject disruptEffectPrefab = Resources.Load<GameObject>("prefabs/effects/smokescreeneffect");
		public bool disruptActive = false;
		public EnemyDisruptComponent currentDisruptTarget = null;
		[SyncVar]
		public int currentDisruptProgress = 0;

		public float rotationAngularVelocity = 40f;
		public float acceleration = 20f;
		public float damping = 0.1f;

		public CharacterBody ownerBody;
		public GameObject ownerBodyObject;
		public GameObject targetBodyObject;

		public SpotterTargetingController targetingController;

		public SpotterMode spotterMode = SpotterMode.ChainLightning;

		[SyncVar]
		public uint __ownerMasterNetID = uint.MaxValue;	//trying to find body on client with this doesn't work
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
	}
}
