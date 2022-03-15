//using NS_KingKombatArena;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SniperClassic.Hooks
{
    public class RecalculateStats
    {
        public static void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SniperContent.spotterStatDebuff))
            {
                args.armorAdd -= 30f;
                if (!SniperClassic.arenaActive || !sender.isPlayerControlled)
                {
                    args.moveSpeedReductionMultAdd += 0.4f;
                }
            }
        }
    }
}
