using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace SniperClassic.Hooks
{
    public class StealBuffVisuals
    {
        public StealBuffVisuals()
        {
            IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "FullCrit")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                {
                    return hasBuff || (self.body.HasBuff(Modules.SniperContent.spotterStatDebuff));
                });
            };
        }
    }
}
