using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    public class SpotterFollowerDistractController : NetworkBehaviour
    {
        [SyncVar]
        public bool currentlyDistracting;

        [SyncVar]
        public Vector3 distractPosition;

        public static int maxPulses = 3;
        public static float timeBetweenPulses = 1f;
        public static float distractRange = 30f;
        public static GameObject effectPrefab;

        private float stopwatch;
        private float age;
        private int pulsesRemaining;

        public void Awake()
        {
            if (NetworkServer.active)
            {
                currentlyDistracting = false;
                distractPosition = Vector3.zero;
            }

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
                if (stopwatch > SpotterFollowerDistractController.timeBetweenPulses)
                {
                    stopwatch -= SpotterFollowerDistractController.timeBetweenPulses;
                    DistractPulse();
                }

                if (pulsesRemaining <= 0)
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
        }

        [Server]
        public void StartDistraction()
        {
            currentlyDistracting = true;
            pulsesRemaining = SpotterFollowerDistractController.maxPulses;
        }

        public void EndDistraction()
        {
            currentlyDistracting = false;
            pulsesRemaining = 0;
            stopwatch = 0f;
        }
    }
}
