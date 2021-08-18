using RoR2;
using RoR2.Orbs;
using SniperClassic.Modules;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

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
                int spotterCount = 0;
                if (victim)
                {
                    victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(SniperContent.spotterScepterBuff) || victimBody.HasBuff(SniperContent.spotterScepterCooldownBuff))
                        {
                            hadSpotter = true;
                            hadSpotterScepter = true;
                            spotterCount = Math.Min(10, victimBody.GetBuffCount(SniperContent.spotterScepterBuff) + victimBody.GetBuffCount(SniperContent.spotterScepterCooldownBuff));
                        }
                        else if (victimBody.HasBuff(SniperContent.spotterBuff) || victimBody.HasBuff(SniperContent.spotterCooldownBuff))
                        {
                            hadSpotter = true;
                            spotterCount = Math.Min(10, victimBody.GetBuffCount(SniperContent.spotterBuff) + victimBody.GetBuffCount(SniperContent.spotterCooldownBuff));
                        }
                    }
                }
                float spotterPercent = (float)spotterCount / 10f;
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
                                    if (hadSpotterScepter)
                                    {
                                        RemoveAllBuffStacks(victimBody, SniperContent.spotterScepterBuff);
                                        RemoveAllBuffStacks(victimBody, SniperContent.spotterScepterCooldownBuff);
                                    }
                                    else
                                    {
                                        RemoveAllBuffStacks(victimBody, SniperContent.spotterBuff);
                                        RemoveAllBuffStacks(victimBody, SniperContent.spotterCooldownBuff);
                                    }
                                }

                                LightningOrb spotterLightning = new LightningOrb
                                {
                                    attacker = damageInfo.attacker,
                                    inflictor = damageInfo.attacker,
                                    damageValue = damageInfo.damage * (hadSpotterScepter ? 1f : 0.5f) * spotterPercent,
                                    procCoefficient = 0.5f,
                                    teamIndex = attackerBody.teamComponent.teamIndex,
                                    isCrit = damageInfo.crit,
                                    procChainMask = damageInfo.procChainMask,
                                    lightningType = LightningOrb.LightningType.Tesla,
                                    damageColorIndex = DamageColorIndex.Nearby,
                                    bouncesRemaining = 5 * (hadSpotterScepter ? 2 : 1),
                                    targetsToFindPerBounce = 5 * (hadSpotterScepter ? 2 : 1),
                                    range = 20f * (hadSpotterScepter ? 2f : 1f) * spotterPercent,
                                    origin = damageInfo.position,
                                    damageType = spotterPercent < 1f ? DamageType.SlowOnHit : (DamageType.SlowOnHit | DamageType.Stun1s),
                                    speed = 120f
                                };

                                spotterLightning.bouncedObjects = new List<HealthComponent>();

                                SpotterLightningController lightningController = damageInfo.attacker.GetComponent<SpotterLightningController>();
                                if (lightningController)
                                {
                                    lightningController.QueueLightning(spotterLightning, 0.1f, spotterPercent);
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void RemoveAllBuffStacks(CharacterBody body, BuffDef buff)
        {
            int count = body.GetBuffCount(buff);
            for (int i = 0; i < count; i++)
            {
                body.RemoveBuff(buff);
            }
        }
    }
}
