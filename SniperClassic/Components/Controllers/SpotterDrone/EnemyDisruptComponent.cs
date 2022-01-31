using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using R2API;
using SniperClassic.Modules;
using RoR2.CharacterAI;
using SniperClassic.Hooks;

namespace SniperClassic.Controllers
{
    public class EnemyDisruptComponent : MonoBehaviour
    {
		public void FixedUpdate()
        {
			if (victimBody.healthComponent && !victimBody.healthComponent.alive)
			{
				Destroy(this);
				return;
			}

			hitStopwatch += Time.fixedDeltaTime;
			if (hitStopwatch > baseHitDelay)
            {
				hitStopwatch -= baseHitDelay;
				DrawAggro(victimBody.healthComponent);
				TriggerDisrupt();
            }
        }

		private void TriggerDisrupt()
		{
			Vector3 position = victimBody.corePosition;

			EffectManager.SpawnEffect(OnHitEnemy.shockExplosionEffect, new EffectData
			{
				origin = position,
				scale = vfxRadius
			}, true);

			hitCounter++;

			if (hitCounter >= baseHitCount)
            {
				Destroy(this);
            }
		}

		public void OnDestroy()
        {
			if (victimBody && victimBody.healthComponent && victimBody.healthComponent.alive)
			{
				RemoveAggro();
			}
        }

		//Based on https://github.com/DestroyedClone/PoseHelper/blob/master/HighPriorityAggroTest/HPATPlugin.cs
		private void DrawAggro(HealthComponent targetHealth)
		{
			float range = aggroRange;
			float attentionDuration = (baseHitCount - hitCounter) * baseHitDelay;

			RaycastHit[] array = Physics.SphereCastAll(victimBody.corePosition, range, Vector3.up, range, RoR2.LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
			foreach (RaycastHit rh in array)
			{
				Collider collider = rh.collider;
				if (collider.gameObject)
				{
					RoR2.HurtBox component = collider.GetComponent<RoR2.HurtBox>();
					if (component)
					{
						RoR2.HealthComponent healthComponent = component.healthComponent;
						if (healthComponent && healthComponent != targetHealth
							&& healthComponent.body.master
							&& healthComponent.body.teamComponent && healthComponent.body.teamComponent.teamIndex == victimTeamIndex)
						{
							if (healthComponent.body.master.aiComponents.Length > 0)
							{
								foreach (BaseAI ai in healthComponent.body.master.aiComponents)
								{
									ai.currentEnemy.gameObject = victimBody.gameObject;
									ai.currentEnemy.bestHurtBox = victimBody.mainHurtBox;
									ai.enemyAttention = ai.enemyAttentionDuration;
									ai.targetRefreshTimer = attentionDuration;
									ai.BeginSkillDriver(ai.EvaluateSkillDrivers());
								}
							}
						}
					}
				}
			}
		}

		private void RemoveAggro()
		{
			float range = aggroRange * (1f);

			RaycastHit[] array = Physics.SphereCastAll(victimBody.corePosition, range, Vector3.up, range, RoR2.LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
			foreach (RaycastHit rh in array)
			{
				Collider collider = rh.collider;
				if (collider.gameObject)
				{
					RoR2.HurtBox component = collider.GetComponent<RoR2.HurtBox>();
					if (component)
					{
						RoR2.HealthComponent healthComponent = component.healthComponent;
						if (healthComponent && healthComponent.body && healthComponent.body.master && healthComponent.body.teamComponent && healthComponent.body.teamComponent.teamIndex == victimTeamIndex)
						{
							if (healthComponent.body.master.aiComponents.Length > 0)
							{
								foreach (BaseAI ai in healthComponent.body.master.aiComponents)
								{
									if (ai.currentEnemy.gameObject == victimBody.gameObject)
									{
										ai.currentEnemy.gameObject = null;
										ai.currentEnemy.bestHurtBox = null;
										ai.BeginSkillDriver(ai.EvaluateSkillDrivers());
									}
								}
							}
						}
					}
				}
			}
		}

		private float hitStopwatch = 0f;
		public int hitCounter = 0;

		public TeamIndex teamIndex;
		public GameObject attacker;
		public CharacterBody attackerBody;

		public CharacterBody victimBody;
		public TeamIndex victimTeamIndex;

		public float baseHitDelay = 0.5f;
		public static int baseHitCount = 6;
        public static float vfxRadius = 2f;
		public static float aggroRange = 30f;
    }
}
