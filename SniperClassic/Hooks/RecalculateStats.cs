using NS_KingKombatArena;
using R2API;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SniperClassic.Hooks
{
    public class RecalculateStats
    {
        public static void AddHook()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SniperContent.spotterBuff) || sender.HasBuff(SniperContent.spotterCooldownBuff) || sender.HasBuff(SniperContent.spotterScepterCooldownBuff) || sender.HasBuff(SniperContent.spotterScepterBuff))
            {
                args.armorAdd += -25f;
                if (!SniperClassic.arenaActive)
                {
                    args.moveSpeedMultAdd -= 0.4f;
                }
            }
        }
    }
}
