using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SniperClassic
{
    public class SpotterFollowerDistractController : NetworkBehaviour
    {
        [SyncVar]
        public bool currentlyDistracting;

        [SyncVar]
        public Vector3 distractPosition;

        public static int maxPulses = 4;
        public static float timeBetweenPulses = 1f;
        public static float distractRange = 40f;
        public static GameObject effectPrefab;
        

        private float stopwatch;
        private int pulsesRemaining;
        private List<BaseAI> affectedAI; 
        public CharacterBody ownerBody;
        public SpotterFollowerController followerController;

        private HurtBox hurtBox;

        public bool SpotterReady()
        {
            return ownerBody.HasBuff(Modules.SniperContent.spotterPlayerReadyBuff);
        }

        public void Awake()
        {
            if (NetworkServer.active)
            {
                currentlyDistracting = false;
                distractPosition = Vector3.zero;
            }

            affectedAI = new List<BaseAI>();
            distractPosition = Vector3.zero;
            stopwatch = 0f;
            pulsesRemaining = SpotterFollowerDistractController.maxPulses;
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }
        }

        [Server]
        public void FixedUpdateServer()
        {
            if (currentlyDistracting)
            {
                stopwatch += Time.fixedDeltaTime;

                if (SpotterReady())
                {
                    if (pulsesRemaining <= 0)
                    {
                        EndDistraction();
                    }
                    else if (stopwatch > SpotterFollowerDistractController.timeBetweenPulses)
                    {
                        stopwatch -= SpotterFollowerDistractController.timeBetweenPulses;
                        DistractPulse();
                    }
                }
                else
                {
                    EndDistraction();
                }
            }
        }

        [Server]
        public void ServerSetDistractPosition(Vector3 position)
        {
            if (!NetworkServer.active) return;
            if (position != distractPosition)
            {
                distractPosition = position;
            }
        }

        [Server]
        private void DistractPulse()
        {
            pulsesRemaining--;
            EffectManager.SpawnEffect(effectPrefab, new EffectData
            {
                origin = base.transform.position,
                scale = 12f
            }, true);
            DrawAggro();
        }

        public void StartDistraction()
        {
            if (!SpotterReady()) return;
            currentlyDistracting = true;
            pulsesRemaining = SpotterFollowerDistractController.maxPulses;
        }

        public void EndDistraction()
        {
            currentlyDistracting = false;
            pulsesRemaining = 0;
            stopwatch = 0f;
            RemoveAggro();
        }

        private void DrawAggro()
        {
            float range = SpotterFollowerDistractController.distractRange;

            CharacterBody targetBody = null;
            GameObject targetObject = null;
            HurtBox targetHurtbox = null;

            if (followerController && followerController.HasTarget())
            {
                targetBody = followerController.GetTargetBody();
                if(targetBody)
                {
                    targetObject = targetBody.gameObject;
                    targetHurtbox = targetBody.mainHurtBox;
                }
            }

            RaycastHit[] array = Physics.SphereCastAll(distractPosition, range, Vector3.up, range, RoR2.LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
            foreach (RaycastHit rh in array)
            {
                Collider collider = rh.collider;
                if (collider.gameObject)
                {
                    RoR2.HurtBox component = collider.GetComponent<RoR2.HurtBox>();
                    if (component)
                    {
                        RoR2.HealthComponent healthComponent = component.healthComponent;
                        if (healthComponent
                            && !healthComponent.body.isChampion
                            && healthComponent.body.master
                            && healthComponent.body.teamComponent && healthComponent.body.teamComponent.teamIndex != ownerBody.teamComponent.teamIndex)
                        {
                            if (!healthComponent.body.isPlayerControlled)
                            {
                                foreach (BaseAI ai in healthComponent.body.master.aiComponents)
                                {
                                    affectedAI.Add(ai);
                                    ai.currentEnemy.gameObject = targetObject;
                                    ai.currentEnemy.bestHurtBox = targetHurtbox;
                                    ai.enemyAttention = timeBetweenPulses + 0.2f;
                                    ai.targetRefreshTimer = timeBetweenPulses + 0.2f;
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
            foreach (BaseAI ai in affectedAI)
            {
                if (ai.currentEnemy.gameObject = base.gameObject)
                {
                    ai.currentEnemy.gameObject = null;
                    ai.currentEnemy.bestHurtBox = null;
                    ai.BeginSkillDriver(ai.EvaluateSkillDrivers());
                }
            }
            affectedAI.Clear();
        }
    }
}
