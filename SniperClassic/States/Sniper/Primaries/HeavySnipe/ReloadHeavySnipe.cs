using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class ReloadHeavySnipe : BaseReloadState
    {
        public override void SetStats()
        {
            internalBaseDuration = baseDuration;
        }
        public static float baseDuration = 0.7f;
    }
}
