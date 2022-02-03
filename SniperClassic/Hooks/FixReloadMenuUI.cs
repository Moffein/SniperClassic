using System;
using System.Collections.Generic;
using System.Text;

namespace SniperClassic.Hooks
{
    public class FixReloadMenuUI
    {
        public FixReloadMenuUI()
        {
            On.RoR2.NetworkUIPromptController.OnControlBegin += (orig, self) =>
            {
                orig(self);

                if (self.currentLocalParticipant.cachedBodyObject)
                {
                    ReloadController rc = self.currentLocalParticipant.cachedBodyObject.GetComponent<ReloadController>();
                    if (rc)
                    {
                        rc.menuActive = true;
                    }
                }
            };

            On.RoR2.NetworkUIPromptController.OnControlEnd += (orig, self) =>
            {
                orig(self);

                if (self.currentLocalParticipant.cachedBodyObject)
                {
                    ReloadController rc = self.currentLocalParticipant.cachedBodyObject.GetComponent<ReloadController>();
                    if (rc)
                    {
                        rc.menuActive = false;
                    }
                }
            };
        }
    }
}
