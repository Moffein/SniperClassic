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
        public static void AddHook()
        {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                if (self.HasBuff(SniperContent.spotterStatDebuff))
                {
                    self.armor -= 25f;
                    if (!SniperClassic.arenaActive)
                    {
                        self.moveSpeed *= 0.6f;
                    }
                }
            };
        }
    }
}
