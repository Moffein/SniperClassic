using EntityStates.GlobalSkills.LunarNeedle;
using EntityStates.SniperClassicSkills;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace SniperClassic.Hooks
{
    public class ScopeNeedleRifle
    {
        public static void AddHook()
        {
            On.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.OnEnter += (orig, self) =>
            {
                if (self.characterBody)
                {
                    ScopeController sc = self.characterBody.gameObject.GetComponent<ScopeController>();
                    if (sc && sc.IsScoped)
					{
						float charge = sc.ShotFired(false);
						float chargeMult = Mathf.Lerp(1f, ScopeController.maxChargeMult, charge);

						self.attackSpeedStat = self.characterBody.attackSpeed;
						self.damageStat = self.characterBody.damage;
						self.critStat = self.characterBody.crit;
						self.moveSpeedStat = self.characterBody.moveSpeed;
						self.duration = FireLunarNeedle.baseDuration / self.attackSpeedStat * 2f;
						if (self.isAuthority)
						{
							Ray aimRay = self.GetAimRay();
							aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, 0f, 0f);
							FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
							fireProjectileInfo.position = aimRay.origin;
							fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
							fireProjectileInfo.crit = self.characterBody.RollCrit();
							fireProjectileInfo.damage = self.characterBody.damage * FireLunarNeedle.damageCoefficient * 2f * chargeMult;
							fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
							fireProjectileInfo.owner = self.gameObject;
							fireProjectileInfo.procChainMask = default(ProcChainMask);
							fireProjectileInfo.force = 0f;
							fireProjectileInfo.useFuseOverride = false;
							fireProjectileInfo.useSpeedOverride = false;
							fireProjectileInfo.target = null;
							fireProjectileInfo.projectilePrefab = ScopeNeedleRifle.projectilePrefab;
							ProjectileManager.instance.FireProjectile(fireProjectileInfo);

							if (self.skillLocator)
                            {
								self.skillLocator.primary.DeductStock(1);
                            }
						}
						self.AddRecoil(-0.4f * FireLunarNeedle.recoilAmplitude, -0.8f * FireLunarNeedle.recoilAmplitude, -0.3f * FireLunarNeedle.recoilAmplitude, 0.3f * FireLunarNeedle.recoilAmplitude);
						self.characterBody.AddSpreadBloom(FireLunarNeedle.spreadBloomValue);
						self.StartAimMode(2f, false);
						EffectManager.SimpleMuzzleFlash(FireLunarNeedle.muzzleFlashEffectPrefab, self.gameObject, "Head", false);
						Util.PlaySound(FireLunarNeedle.fireSound, self.gameObject);
						self.PlayAnimation(self.animationLayerName, self.animationStateName, self.playbackRateParam, self.duration);

						return;
					}
                }
                orig(self);
            };
        }

		public static GameObject projectilePrefab;
    }
}
