using RoR2;
using RoR2.Orbs;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace SniperClassic.Hooks
{
    public class OnHitEnemy
    {
        public static void AddHook()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                CharacterBody victimBody = null;
                bool hadSpotter = false;
                if (victim)
                {
                    victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(SniperContent.spotterBuff) || victimBody.HasBuff(SniperContent.spotterCooldownBuff))
                        {
                            if (victimBody.HasBuff(SniperContent.spotterBuff))
                            {
                                hadSpotter = true;
                            }
                        }
                    }
                }
                orig(self, damageInfo, victim);
                if (!damageInfo.rejected && hadSpotter)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            if (damageInfo.procCoefficient > 0f && !(damageInfo.damage / attackerBody.damage < 4f))
                            {

                                if (victimBody && victimBody.healthComponent && victimBody.healthComponent.alive)
                                {
                                    victimBody.RemoveBuff(SniperContent.spotterBuff);
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        victimBody.AddTimedBuff(SniperContent.spotterCooldownBuff, i);
                                    }
                                }

                                LightningOrb spotterLightning = new LightningOrb
                                {
                                    attacker = damageInfo.attacker,
                                    inflictor = damageInfo.attacker,
                                    damageValue = damageInfo.damage * 0.5f,
                                    procCoefficient = 0.5f,
                                    teamIndex = attackerBody.teamComponent.teamIndex,
                                    isCrit = damageInfo.crit,
                                    procChainMask = damageInfo.procChainMask,
                                    lightningType = LightningOrb.LightningType.Tesla,
                                    damageColorIndex = DamageColorIndex.WeakPoint,
                                    bouncesRemaining = 5,
                                    targetsToFindPerBounce = 5,
                                    range = 20f,
                                    origin = damageInfo.position,
                                    damageType = (DamageType.SlowOnHit | DamageType.Stun1s),
                                    speed = 120f
                                };

                                spotterLightning.bouncedObjects = new List<HealthComponent>();

                                SpotterLightningController stc = damageInfo.attacker.GetComponent<SpotterLightningController>();
                                if (stc)
                                {
                                    stc.QueueLightning(spotterLightning, 0.1f);
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
