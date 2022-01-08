using EntityStates.SniperClassicSkills;
using NS_KingKombatArena;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SniperClassic.Hooks
{
    public class Stage_Start
    {
        public Stage_Start()
        {
            if (SniperClassic.arenaPluginLoaded)
            {
                On.RoR2.Stage.Start += (orig, self) =>
                {
                    orig(self);
                    SetArena();
                };
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void SetArena()
        {
            SniperClassic.arenaActive = KingKombatArenaMainPlugin.s_GAME_MODE_ACTIVE;
        }
    }
}
