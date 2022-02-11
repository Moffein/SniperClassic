using EntityStates.SniperClassicSkills;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SniperClassic
{
	public class ScopeChargeIndicatorController : MonoBehaviour
	{
		private void Awake()
		{
			this.hudElement = base.GetComponent<HudElement>();
			this.image = base.GetComponent<Image>();
		}

		private void FixedUpdate()
		{
			if (this.hudElement.targetCharacterBody)
			{
				SkillLocator component = this.hudElement.targetCharacterBody.GetComponent<SkillLocator>();
				if (component && component.secondary)
				{
					EntityStateMachine stateMachine = component.secondary.stateMachine;
					if (stateMachine)
					{
                        SecondaryScope scopeSniper = stateMachine.state as SecondaryScope;
						if (scopeSniper != null && scopeSniper.scopeComponent != null && scopeSniper.scopeComponent.IsScoped)
						{
							if (component.secondary.stock > 0)
                            {
								image.color = scopeSniper.scopeComponent.charge < 1f ? chargeColor : fullChargeColor;
								image.fillAmount = scopeSniper.scopeComponent.charge / scopeSniper.scopeComponent.GetMaxCharge();
							}
							else
                            {
								image.color = rechargeColor;
								image.fillAmount = 1f - component.secondary.rechargeStopwatch / component.secondary.CalculateFinalRechargeInterval();
							}
						}
					}
				}
			}
		}

		private HudElement hudElement;

		public Image image;

		public static Color chargeColor = new Color(167f / 255f, 125f / 255f, 1f, 186f / 255f);
		public static Color fullChargeColor = new Color(1f, 1f, 1f, 186f / 255f);
		public static Color rechargeColor = new Color(180f / 255f, 0f, 0f, 186f / 255f);
	}
}
