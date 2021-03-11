using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class ReloadSnipe : BaseReloadState
    {
        public override void SetStats()
        {
            internalBaseDuration = baseDuration;
        }
        public static float baseDuration = 0.6f;
    }
}
