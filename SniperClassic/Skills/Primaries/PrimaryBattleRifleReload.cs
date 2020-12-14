using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class ReloadBattleRifle : ReloadSnipe
    {
        public override void DoReload()
        {
            base.DoReload();
            this.reloadComponent.hideLoadIndicator = true;
            base.skillLocator.primary.stock = reloadComponent.GetMagSize();
        }

        public override void AutoReload()
        {
            base.AutoReload();
            base.skillLocator.primary.stock = reloadComponent.GetMagSize();
        }
        public static float reloadBarLength = 1f;
        public static float baseDuration = 0.5f;
    }
}
