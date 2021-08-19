using EntityStates;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace SniperClassic.Hooks
{
    class OnEnter
    {
        public static void AddHook()
        {
            On.EntityStates.BaseState.OnEnter += (orig, self) =>
            {
                orig(self);
                if (self.characterBody && self.characterBody.HasBuff(SniperContent.trickshotBuff))
                {
                    self.damageStat *= 1f + 1.5f * self.characterBody.GetBuffCount(SniperContent.trickshotBuff);
                }
            };
        }
    }
}
