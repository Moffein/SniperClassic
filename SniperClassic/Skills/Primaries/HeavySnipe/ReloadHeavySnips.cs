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
            internalScaleReloadSpeed = scaleReloadSpeed;
        }
        public static float baseDuration = 0.8f;
        public static bool scaleReloadSpeed = false;
    }
}
