using NS_KingKombatArena;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SniperClassic.Hooks
{
    public class RecalculateStats
    {
        public RecalculateStats()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender.HasBuff(SniperContent.spotterStatDebuff))
                {
                    args.armorAdd -= 25f;
                    if (!SniperClassic.arenaActive)
                    {
                        args.moveSpeedReductionMultAdd += 0.4f;
                    }
                }
            };
        }
    }
}
