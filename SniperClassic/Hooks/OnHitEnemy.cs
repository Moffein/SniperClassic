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
                bool hadSpotterScepter = false;
                if (victim)
                {
                    victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(SniperContent.spotterBuff) || victimBody.HasBuff(SniperContent.spotterScepterBuff))
                        {
                            hadSpotter = true;
                            if (victimBody.HasBuff(SniperContent.spotterScepterBuff))
                            {
                                hadSpotterScepter = true;
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
                                    if (victimBody.HasBuff(SniperContent.spotterBuff))
                                    {
                                        victimBody.RemoveBuff(SniperContent.spotterBuff);
                                    }
                                    if (victimBody.HasBuff(SniperContent.spotterScepterBuff))
                                    {
                                        victimBody.RemoveBuff(SniperContent.spotterScepterBuff);
                                    }

                                    for (int i = 1; i <= 10; i++)
                                    {
                                        victimBody.AddTimedBuff(SniperContent.spotterCooldownBuff, i);
                                    }
                                }

                                LightningOrb spotterLightning = new LightningOrb
                                {
                                    attacker = damageInfo.attacker,
                                    inflictor = damageInfo.attacker,
                                    damageValue = damageInfo.damage * (hadSpotterScepter ? 1f : 0.5f),
                                    procCoefficient = 0.5f,
                                    teamIndex = attackerBody.teamComponent.teamIndex,
                                    isCrit = damageInfo.crit,
                                    procChainMask = damageInfo.procChainMask,
                                    lightningType = LightningOrb.LightningType.Tesla,
                                    damageColorIndex = DamageColorIndex.Nearby,
                                    bouncesRemaining = 5 * (hadSpotterScepter ? 2 : 1),
                                    targetsToFindPerBounce = 5 * (hadSpotterScepter ? 2 : 1),
                                    range = 20f * (hadSpotterScepter ? 2f : 1f),
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
