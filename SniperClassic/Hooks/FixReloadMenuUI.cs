using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RoR2;

namespace SniperClassic.Hooks
{
    public class FixReloadMenuUI
    {
        //The participant can already be cleared by the time OnControlEnd runs (e.g. finishing a trade with SS2's ZanZan),
        //so remember which ReloadController each prompt disabled instead of looking it up again.
        private static readonly ConditionalWeakTable<NetworkUIPromptController, ReloadController> activeMenus = new ConditionalWeakTable<NetworkUIPromptController, ReloadController>();

        public FixReloadMenuUI()
        {
            On.RoR2.NetworkUIPromptController.OnControlBegin += (orig, self) =>
            {
                orig(self);

                UnityEngine.GameObject bodyObject = self.currentLocalParticipant?.cachedBodyObject;
                if (bodyObject)
                {
                    ReloadController rc = bodyObject.GetComponent<ReloadController>();
                    if (rc)
                    {
                        rc.menuActive = true;
                        activeMenus.Remove(self);
                        activeMenus.Add(self, rc);
                    }
                }
            };

            On.RoR2.NetworkUIPromptController.OnControlEnd += (orig, self) =>
            {
                orig(self);

                if (activeMenus.TryGetValue(self, out ReloadController rc))
                {
                    activeMenus.Remove(self);
                    if (rc)
                    {
                        rc.menuActive = false;
                    }
                }
            };
        }
    }
}
