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
			if (NetworkServer.active)
			{
				this.FixedUpdateServer();
			}
			else
            {
				if (this.cachedTargetMasterNetID != this.targetMasterNetID)
				{
					this.cachedTargetMasterNetID = this.targetMasterNetID;
					this.cachedTargetBodyObject = FindBodyOnClient(this.cachedTargetMasterNetID);
					this.OnTargetChanged();
				}
			}
		}

		[Server]
		public void AssignNewTarget(GameObject target, uint netID)
		{
			if (!NetworkServer.active)
			{
				return;
			}

			if (target != this.targetBodyObject && target != ownerBodyObject && this.cachedTargetBody)
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

			targetMasterNetID = (target ? netID : ownerMasterNetID);
			this.targetBodyObject = (target ? target : this.ownerBodyObject);
			this.cachedTargetBodyObject = this.targetBodyObject;
			this.OnTargetChanged();
			/*if (this.targetBodyObject.GetComponent<CharacterBody>())
			{
				EffectManager.SimpleImpactEffect(this.burstHealEffect, this.GetTargetPosition(), Vector3.up, true);
			}*/

			ApplyDebuff();
		}

		private void OnTargetChanged()
		{
			//this.cachedTargetHealthComponent = (this.cachedTargetBodyObject ? this.cachedTargetBodyObject.GetComponent<HealthComponent>() : null);
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
				targetMasterNetID = ownerMasterNetID;
			}
			if (!this.ownerBodyObject)
			{
				this.ownerBodyObject = FindBodyOnClient(ownerMasterNetID);
				if (!this.ownerBodyObject)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}

			ApplyDebuff();
		}

		public GameObject FindBodyOnClient(uint i)
        {
			if (i == ownerMasterNetID)
            {
				return ownerBodyObject;
            }

			GameObject find = ClientScene.FindLocalObject(new NetworkInstanceId(ownerMasterNetID));
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
			base.transform.rotation = Quaternion.AngleAxis(this.rotationAngularVelocity * Time.deltaTime, Vector3.up) * base.transform.rotation;
			/*if (this.targetBodyObject)
			{
				this.indicator.transform.position = this.GetTargetPosition();
			}*/
		}

		/*[Server]
		private void DoHeal(float healFraction)
		{

			Debug.Log("Starting doheal");
			if (!NetworkServer.active)
			{
				return;
			}
			if (!this.cachedTargetHealthComponent)
			{
				Debug.Log("no cached target health component");
				return;
			}
			this.cachedTargetHealthComponent.HealFraction(healFraction, default(ProcChainMask));
		}*/

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
			base.transform.position = this.GetDesiredPosition();
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

		private Vector3 GetDesiredPosition()
		{
			return this.GetTargetPosition();
		}

		private void UpdateMotion()
		{
			Vector3 desiredPosition = this.GetDesiredPosition();
			if (this.enableSpringMotion)
			{
				Vector3 lhs = desiredPosition - base.transform.position;
				if (lhs != Vector3.zero)
				{
					Vector3 a = lhs.normalized * this.acceleration;
					Vector3 b = this.velocity * -this.damping;
					this.velocity += (a + b) * Time.deltaTime;
					return;
				}
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, desiredPosition, ref this.velocity, this.damping);
			}
		}

		/*public float fractionHealthHealing = 0.01f;
		public float fractionHealthBurst = 0.05f;
		public float healingInterval = 1f;*/
		public float rotationAngularVelocity = 30f;
		public float acceleration = 20f;
		public float damping = 0.1f;
		public bool enableSpringMotion = false;
		//public GameObject indicator;

		public GameObject ownerBodyObject;
		public GameObject targetBodyObject;

		[SyncVar]
		public uint ownerMasterNetID;
		[SyncVar]
		public uint targetMasterNetID;

		public uint cachedTargetMasterNetID;

		//public GameObject burstHealEffect;
		//public GameObject indicator;
		private GameObject cachedTargetBodyObject;
		private HealthComponent cachedTargetHealthComponent;
		private CharacterBody cachedTargetBody;
		private float healingTimer;
		private Vector3 velocity = Vector3.zero;

		private NetworkInstanceId ___ownerBodyObjectNetId;
		private NetworkInstanceId ___targetBodyObjectNetId;
	}
}
