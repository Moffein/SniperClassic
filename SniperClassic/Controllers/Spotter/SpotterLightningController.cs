using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    class SpotterLightningController : MonoBehaviour
    {
        public void QueueLightning(LightningOrb lightningOrb, float delay, float scale)
        {
            effectScale = scale;
            lightningDelayStopwatch = delay;
            queuedLightningOrb = lightningOrb;
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (lightningDelayStopwatch > 0)
                {
                    lightningDelayStopwatch -= Time.fixedDeltaTime;
                    if (lightningDelayStopwatch <= 0)
                    {
                        if (queuedLightningOrb != null)
                        {
                            HurtBox hurtBox = this.queuedLightningOrb.PickNextTarget(this.queuedLightningOrb.origin);
                            while (hurtBox && hurtBox.healthComponent && !hurtBox.healthComponent.alive)
                            {
                                this.queuedLightningOrb.bouncedObjects.Add(hurtBox.healthComponent);
                                hurtBox = queuedLightningOrb.PickNextTarget(queuedLightningOrb.origin);
                            }
                            if (hurtBox)
                            {
                                this.queuedLightningOrb.target = hurtBox;
                                OrbManager.instance.AddOrb(queuedLightningOrb);
                            }
                            EffectManager.SpawnEffect(SpotterLightningController.shockExplosionEffect, new EffectData
                            {
                                origin = queuedLightningOrb.origin,
                                scale = queuedLightningOrb.range * effectScale
                            }, true);
                        }
                        this.queuedLightningOrb = null;
                    }
                }
            }
        }

        public static GameObject shockExplosionEffect;
        private float lightningDelayStopwatch;
        private LightningOrb queuedLightningOrb;
        private float effectScale;
    }
}
