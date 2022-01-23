using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    public class SpotterRechargeController : MonoBehaviour
    {
        public static float baseRechargeDuration = 10f;
        public static GameObject spotterReadyEffectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/omniimpactvfxloader");

        public CharacterBody ownerBody;
        public float rechargeStopwatch;

        private SpotterTargetingController targetingController;
        private bool hadSpotterReady;

        public bool SpotterReady()
        {
            return ownerBody.HasBuff(Modules.SniperContent.spotterPlayerReadyBuff.buffIndex);
        }

        public int GetCooldown()
        {
            return ownerBody.GetBuffCount(Modules.SniperContent.spotterPlayerCooldownBuff.buffIndex);
        }

        public void TriggerSpotter()
        {
            rechargeStopwatch = 0f;
            int buffCount = Mathf.CeilToInt(baseRechargeDuration);
            for (int i = 0; i < buffCount; i++)
            {
                ownerBody.AddTimedBuff(Modules.SniperContent.spotterPlayerCooldownBuff.buffIndex, 10000f);   //Must be timed buff so that it can be cleared by Blast Shower;
            }

            BuffIndex readyBuff = Modules.SniperContent.spotterPlayerReadyBuff.buffIndex;
            if (ownerBody.HasBuff(readyBuff))
            {
                ownerBody.RemoveBuff(readyBuff);
            }
        }

        public void Awake()
        {
            ownerBody = base.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
                return;
            }
            rechargeStopwatch = baseRechargeDuration;
            hadSpotterReady = false;
            targetingController = base.GetComponent<SpotterTargetingController>();
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }

            bool spotterReady = SpotterReady();
            if (!hadSpotterReady && spotterReady)
            {
                Util.PlaySound("Play_item_proc_crit_cooldown", ownerBody.gameObject);
                if (targetingController && targetingController.spotterFollower)
                {
                    EffectManager.SimpleEffect(spotterReadyEffectPrefab, targetingController.spotterFollower.gameObject.transform.position, default, false);
                }
            }
            hadSpotterReady = spotterReady;
        }

        //Fully re-evaluate buffs at every step.
        public void FixedUpdateServer()
        {
            if (rechargeStopwatch < baseRechargeDuration)
            {
                rechargeStopwatch += Time.fixedDeltaTime * ownerBody.attackSpeed;
            }

            BuffIndex cooldownBuff = Modules.SniperContent.spotterPlayerCooldownBuff.buffIndex;
            BuffIndex readyBuff = Modules.SniperContent.spotterPlayerReadyBuff.buffIndex;

            bool hasCooldown = ownerBody.HasBuff(cooldownBuff);
            bool hasReady = ownerBody.HasBuff(readyBuff);

            float cooldownPercent = rechargeStopwatch / baseRechargeDuration;

            //If cooldown was cleared via Blast Shower, skip straight to removing the cooldown.
            if (hasCooldown && cooldownPercent < 1f)
            {
                if (hasReady)
                {
                    ownerBody.RemoveBuff(readyBuff);
                }

                int buffCount =  Mathf.CeilToInt(baseRechargeDuration * (1 - cooldownPercent));
                int currentBuffs = ownerBody.GetBuffCount(cooldownBuff);

                if (buffCount != currentBuffs)
                {
                    ownerBody.ClearTimedBuffs(cooldownBuff);
                    for (int i = 0; i< buffCount; i++)
                    {
                        ownerBody.AddTimedBuff(cooldownBuff, 10000f);   //Must be timed buff so that it can be cleared by Blast Shower;
                    }
                }
            }
            else
            {
                rechargeStopwatch = baseRechargeDuration;
                if (hasCooldown)
                {
                    ownerBody.ClearTimedBuffs(cooldownBuff);
                }
                if (!hasReady)
                {
                    ownerBody.AddBuff(readyBuff);
                }
            }
        }
    }
}
