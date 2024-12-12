using EntityStates.SniperClassicSkills;
using NS_KingKombatArena;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SniperClassic.Hooks
{
    public class DetectArenaMode
    {
        public DetectArenaMode()
        {
            if (SniperClassic.arenaPluginLoaded)
            {
                RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            }
        }

        private void Stage_onStageStartGlobal(RoR2.Stage obj)
        {
            SetArena();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void SetArena()
        {
            SniperClassic.arenaActive = KingKombatArenaMainPlugin.s_GAME_MODE_ACTIVE;
        }
    }
}
