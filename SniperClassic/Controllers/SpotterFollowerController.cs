using EntityStates.Missions.Arena.NullWard;
using RewiredConsts;
using RoR2;
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
					if (this.cachedTargetBody.HasBuff(SniperClassic.spotterBuff))
					{
						this.cachedTargetBody.RemoveBuff(SniperClassic.spotterBuff);
					}
					if (this.cachedTargetBody.HasBuff(SniperClassic.spotterStatDebuff))
					{
						this.cachedTargetBody.RemoveBuff(SniperClassic.spotterStatDebuff);
					}
				}
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
				if (this.cachedTargetBody.HasBuff(SniperClassic.spotterBuff))
				{
					this.cachedTargetBody.RemoveBuff(SniperClassic.spotterBuff);
				}
				if (this.cachedTargetBody.HasBuff(SniperClassic.spotterStatDebuff))
				{
					this.cachedTargetBody.RemoveBuff(SniperClassic.spotterStatDebuff);
				}
			}
			
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
			/*if (this.targetBodyObject.GetComponent<CharacterBody>())
			{
				EffectManager.SimpleImpactEffect(this.burstHealEffect, this.GetTargetPosition(), Vector3.up, true);
			}*/

			ApplyDebuff();
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
						if (this.cachedTargetBody.HasBuff(SniperClassic.spotterBuff))
						{
							this.cachedTargetBody.RemoveBuff(SniperClassic.spotterBuff);
						}
						if (this.cachedTargetBody.HasBuff(SniperClassic.spotterStatDebuff))
						{
							this.cachedTargetBody.RemoveBuff(SniperClassic.spotterStatDebuff);
						}
					}

					UnityEngine.Object.Destroy(base.gameObject);
				}
			}

			ApplyDebuff();
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
			if (!this.cachedTargetBody.HasBuff(SniperClassic.spotterBuff) && !this.cachedTargetBody.HasBuff(SniperClassic.spotterCooldownBuff))
            {
				this.cachedTargetBody.AddBuff(SniperClassic.spotterBuff);
			}
			if (!this.cachedTargetBody.HasBuff(SniperClassic.spotterStatDebuff))
            {
				this.cachedTargetBody.AddBuff(SniperClassic.spotterStatDebuff);
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

		public float rotationAngularVelocity = 40f;
		public float acceleration = 20f;
		public float damping = 0.1f;

		public CharacterBody ownerBody;
		public GameObject ownerBodyObject;
		public GameObject targetBodyObject;

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
		private Vector3 enemyScale = new Vector3(100,100,100);
		private Vector3 playerScale = new Vector3(50, 50, 50);
	}
}
