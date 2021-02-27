using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using SniperClassic;

namespace EntityStates.SniperClassicSkills
{
	public class NeedleRifle : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Debug.Log("Entering!");
			charge = 0f;
			scopeComponent = base.GetComponent<SniperClassic.ScopeController>();
			if (scopeComponent)
			{
				charge = scopeComponent.ShotFired(false);
			}
			reloadComponent = base.GetComponent<SniperClassic.ReloadController>();
			if (reloadComponent)
			{
				reloadComponent.hideLoadIndicator = true;
			}

			this.duration = NeedleRifle.baseDuration / this.attackSpeedStat;
			if (base.isAuthority)
			{
				float chargeMult = Mathf.Lerp(1f, maxChargeMult, this.charge);

				Ray aimRay = base.GetAimRay();
				aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, NeedleRifle.maxSpread, 1f, 1f, 0f, 0f);
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
				fireProjectileInfo.crit = base.characterBody.RollCrit();
				fireProjectileInfo.damage = base.characterBody.damage * NeedleRifle.damageCoefficient * chargeMult;
				fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.procChainMask = default(ProcChainMask);
				fireProjectileInfo.force = 0f;
				fireProjectileInfo.useFuseOverride = false;
				fireProjectileInfo.useSpeedOverride = false;
				fireProjectileInfo.target = null;
				fireProjectileInfo.projectilePrefab = NeedleRifle.projectilePrefab;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
			base.AddRecoil(-0.4f * NeedleRifle.recoilAmplitude, -0.8f * NeedleRifle.recoilAmplitude, -0.3f * NeedleRifle.recoilAmplitude, 0.3f * NeedleRifle.recoilAmplitude);
			base.characterBody.AddSpreadBloom(NeedleRifle.spreadBloomValue);
			base.StartAimMode(2f, false);
			EffectManager.SimpleMuzzleFlash(NeedleRifle.muzzleFlashEffectPrefab, base.gameObject, "Head", false);
			Util.PlaySound(NeedleRifle.fireSound, base.gameObject);
		}

		// Token: 0x06003C7B RID: 15483 RVA: 0x0002BE81 File Offset: 0x0002A081
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
			}
		}

		// Token: 0x06003C7C RID: 15484 RVA: 0x0000425B File Offset: 0x0000245B
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		public static float baseDuration = 0.18f; //set to 0.22f if you want no total dps increase
		public static float damageCoefficient = 2.4f/24f;
		public static GameObject projectilePrefab;
		public static float recoilAmplitude = 1.5f;
		public static float spreadBloomValue = 0f;
		public static GameObject muzzleFlashEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashLunarNeedle");
		public static string fireSound = "Play_item_lunar_primaryReplace_shoot";
		public static float maxSpread = 0f;
		public static float maxChargeMult = 4f;

		private float duration;
		private ScopeController scopeComponent;
		private ReloadController reloadComponent;
		private float charge;
	}
}
