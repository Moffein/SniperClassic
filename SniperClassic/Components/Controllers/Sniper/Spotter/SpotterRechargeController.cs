using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static SniperClassic.Modules.SniperContent.Skills;

namespace SniperClassic
{
    public class SpotterRechargeController : MonoBehaviour
    {
        public static float baseRechargeDuration = 10f;
        public static GameObject spotterReadyEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/omniimpactvfxloader");
        public static bool lysateStack = true;
        public static bool scaleWithAttackSpeed = false;

        public CharacterBody ownerBody;
        public float rechargeStopwatch;

        private SpotterTargetingController targetingController;
        private bool hadSpotterReady;

        public void LowerCooldown(float percent)
        {
            if (rechargeStopwatch < baseRechargeDuration)
            {
                rechargeStopwatch += percent * baseRechargeDuration;
            }
        }

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
                RoR2.Util.PlaySound("Play_item_proc_crit_cooldown", ownerBody.gameObject);
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
                DeductSpotterCooldownServer(Time.fixedDeltaTime);
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

                //Jank.
                GenericSkill special = ownerBody.skillLocator.special;
                float trueRechargeInterval = Mathf.Max(0f, special.baseSkill.baseRechargeInterval * special.cooldownScale - special.flatCooldownReduction) + special.temporaryCooldownPenalty;

                int buffCount =  Mathf.CeilToInt(baseRechargeDuration * (trueRechargeInterval / (special.baseSkill.baseRechargeInterval > 0f ? special.baseSkill.baseRechargeInterval : 0.5f)) * (1 - cooldownPercent));
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
                ResetSpotterCooldownServer();
            }
        }

        public void ResetSpotterCooldownServer()
        {
            if (!NetworkServer.active) return;
            rechargeStopwatch = baseRechargeDuration;
            bool hasCooldown = ownerBody.HasBuff(Modules.SniperContent.spotterPlayerCooldownBuff);
            bool hasReady = ownerBody.HasBuff(Modules.SniperContent.spotterPlayerReadyBuff);
            if (hasCooldown)
            {
                ownerBody.ClearTimedBuffs(Modules.SniperContent.spotterPlayerCooldownBuff);
            }
            if (!hasReady)
            {
                ownerBody.AddBuff(Modules.SniperContent.spotterPlayerReadyBuff);
            }
        }

        //This won't work if called by a client. Will this be an issue?
        public void DeductSpotterCooldownServer(float amount)
        {
            if (!NetworkServer.active) return;
            GenericSkill special = ownerBody.skillLocator.special;
            //Jank. This is used to affect how much rechargeStopwatch gets ticked.
            float trueRechargeInterval = Mathf.Max(0f, special.baseSkill.baseRechargeInterval * special.cooldownScale - special.flatCooldownReduction) + special.temporaryCooldownPenalty;

            float lysateSpeedup = 1f;
            if (SpotterRechargeController.lysateStack)
            {
                if (ownerBody.skillLocator && ownerBody.skillLocator.special && ownerBody.skillLocator.special.maxStock > 0)
                {
                    lysateSpeedup /= Mathf.Pow(0.85f, (ownerBody.skillLocator.special.maxStock - 1));
                }
            }

            float scalar = scaleWithAttackSpeed ? ownerBody.attackSpeed : (trueRechargeInterval / special.baseSkill.baseRechargeInterval);

            rechargeStopwatch += amount * scalar * lysateSpeedup;
        }
    }
}
