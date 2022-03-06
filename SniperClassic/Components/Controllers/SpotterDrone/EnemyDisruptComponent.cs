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
			if (hitStopwatch > scaledHitDelay)
            {
				hitStopwatch -= scaledHitDelay;
				DrawAggro(victimBody.healthComponent);
				TriggerDisrupt();
            }
        }

		private void TriggerDisrupt()
		{
			Vector3 position = victimBody.corePosition;
			EffectManager.SpawnEffect(effectPrefab, new EffectData
			{
				origin = position,
				scale = radius
			}, true);

			EffectManager.SpawnEffect(OnHitEnemy.shockExplosionEffect, new EffectData
			{
				origin = position,
				scale = radius
			}, true);

			BlastAttack ba = new BlastAttack
			{
				radius = radius * (scepter ? 2f : 1f),
				procCoefficient = (scepter ? 1f : 0.5f),
				position = position,
				attacker = attacker,
				crit = attackerBody.RollCrit(),
				baseDamage = attackerBody.damage * damageCoefficient * (scepter ? 2f : 1f),
				falloffModel = BlastAttack.FalloffModel.None,
				baseForce = 0f,
				teamIndex = teamIndex,
				damageType = SniperClassic.arenaActive ? DamageType.SlowOnHit : (scepter? DamageType.Shock5s : DamageType.Stun1s),
				attackerFiltering = AttackerFiltering.NeverHitSelf
			};
			ba.AddModdedDamageType(SniperContent.SpotterDebuffOnHit);
			ba.Fire();

			hitCounter++;

			if (hitCounter >= scaledHitCount)
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
			float range = aggroRange * (scepter ? 2f : 1f);
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
			float range = aggroRange * (scepter ? 2f : 1f);

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

		public bool scepter;
		public TeamIndex teamIndex;
		public GameObject attacker;
		public CharacterBody attackerBody;

		public CharacterBody victimBody;
		public TeamIndex victimTeamIndex;

		public float scaledHitDelay;
		public float scaledHitCount;

		public static float baseHitDelay = 1f;
		public static int baseHitCount = 7;
        public static float damageCoefficient = 1f;
        public static float radius = 15f;
        public static float procCoefficient = 0.5f;
		public static float aggroRange = 40f;

		public static GameObject effectPrefab;
    }
}
