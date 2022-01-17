using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    public class SpotterRechargeController : MonoBehaviour
    {
        public static float baseRechargeDuration = 10f;

        public CharacterBody ownerBody;
        public float rechargeStopwatch;

        public void Awake()
        {
            rechargeStopwatch = baseRechargeDuration;
            ownerBody = base.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }
        }

        public void FixedUpdateServer()
        {
            if (rechargeStopwatch < baseRechargeDuration)
            {
                rechargeStopwatch += Time.fixedDeltaTime * ownerBody.attackSpeed;
            }

            BuffIndex cooldownBuff = Modules.SniperContent.spotterPlayerReadyBuff.buffIndex;
            BuffIndex readyBuff = Modules.SniperContent.spotterPlayerReadyBuff.buffIndex;

            bool hasCooldown = ownerBody.HasBuff(cooldownBuff);
            bool hasReady = ownerBody.HasBuff(readyBuff);

            float cooldownPercent = rechargeStopwatch / baseRechargeDuration;

            //If cooldown was cleared via Blast Shower, skip straight ro removing the cooldown.
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
