using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class ReloadSnipe2 : BaseReloadState
    {
        public override void SetStats()
        {
            internalBaseDuration = baseDuration;
            internalScaleReloadSpeed = scaleReloadSpeed;
            internalReloadBarLength = reloadBarLength;
        }
        public static float baseDuration = 0.4f;
        public static bool scaleReloadSpeed = false;
        public static float reloadBarLength = 0.6f;
    }
}
