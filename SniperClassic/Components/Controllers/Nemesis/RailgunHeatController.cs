using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic.Controllers
{
    public class RailgunHeatController : NetworkBehaviour
    {
        public static float baseCooldownDuration = 100f/(60f*0.3f);
        public static float overheatCooldownRatePenalty = 0.5f;
        public static float maxHeat = 100f;
        public static float heatDecayDelay = 0.3f;

        private float rapidCooldownStopwatch;
        private float heatDecayDelayStopwatch;

        public float gunHeatPercent { get; private set; }
        public float gunHeat { get; private set; }
        public bool overheated { get; private set; }
        public bool empty { get; private set; }

        public void Awake()
        {
            rapidCooldownStopwatch = 0f;
            heatDecayDelayStopwatch = 0f;
            gunHeat = 0f;
            gunHeatPercent = 0f;
            overheated = false;
            empty = false;
        }

        //Didn't test decay in-game, need to see if the rate is right.
        public void FixedUpdate()
        {
            if (base.hasAuthority)
            {
                if (gunHeat > 0f)
                {
                    if (heatDecayDelayStopwatch > 0f)
                    {
                        heatDecayDelayStopwatch -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        if (rapidCooldownStopwatch > 0f)
                        {
                            float downValue = Mathf.Min(1.5f * rapidCooldownStopwatch, 1f);
                            gunHeat -= 120f * Time.fixedDeltaTime * downValue;
                            rapidCooldownStopwatch -= Time.fixedDeltaTime;
                        }
                        gunHeat -= RailgunHeatController.maxHeat * (overheated ? overheatCooldownRatePenalty : 1f) * Time.fixedDeltaTime / baseCooldownDuration;
                        if (gunHeat <= 0f)
                        {
                            ResetHeat();
                        }
                    }
                }
                UpdateGunHeatPerccent();
            }
        }

        private void UpdateGunHeatPerccent()
        {
            gunHeatPercent = gunHeat / RailgunHeatController.maxHeat;
        }

        private void ResetHeat()
        {
            overheated = false;
            gunHeat = 0f;
            heatDecayDelayStopwatch = 0f;
            rapidCooldownStopwatch = 0f;
            empty = true;

            CmdUpdateOverheat(overheated);
            CmdUpdateEmpty(empty);

            UpdateGunHeatPerccent();
        }

        public void AddHeat(float toAdd)
        {
            gunHeat += toAdd;
            CmdUpdateEmpty(empty);
            if (gunHeat >= maxHeat)
            {
                gunHeat = maxHeat;
                overheated = true;
                heatDecayDelayStopwatch = heatDecayDelay;

                CmdUpdateOverheat(overheated);
            }
            UpdateGunHeatPerccent();
        }

        public float DischargeRailgunSingle(float cdStopwatch)
        {
            rapidCooldownStopwatch = cdStopwatch;
            return gunHeatPercent;
        }
        
        [Command]
        private void CmdUpdateEmpty(bool newValue)
        {
            if (base.hasAuthority)
            {
                RpcUpdateEmpty(newValue);
            }
        }

        [ClientRpc]
        private void RpcUpdateEmpty(bool newValue)
        {
            if (!base.hasAuthority)
            {
                empty = newValue;
            }
        }

        [Command]
        private void CmdUpdateOverheat(bool newValue)
        {
            if (base.hasAuthority)
            {
                RpcUpdateOverheat(newValue);
            }
        }

        [ClientRpc]
        private void RpcUpdateOverheat(bool newValue)
        {
            if (!base.hasAuthority)
            {
                overheated = newValue;
            }
        }
    }
}
