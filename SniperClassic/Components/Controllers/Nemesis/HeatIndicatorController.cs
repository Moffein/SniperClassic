using EntityStates.SniperClassicSkills;
using RoR2;
using RoR2.UI;
using SniperClassic.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SniperClassic
{
	public class RailgunHeatIndicatorController : MonoBehaviour
	{
		public void Awake()
		{
			this.hudElement = base.GetComponent<HudElement>();
			this.image = base.GetComponent<Image>();
			heatController = this.hudElement.targetCharacterBody.gameObject.GetComponent<RailgunHeatController>();	//Todo: check if this errors
		}

		public void FixedUpdate()
		{
			if (heatController)
			{
				image.color = heatController.overheated ? overheatColor : chargeColor;
				image.fillAmount = heatController.gunHeatPercent;
			}
		}

		private HudElement hudElement;
		public Image image;
		private RailgunHeatController heatController;

		public static Color chargeColor = new Color(255f / 255f, 255f / 255f, 94f, 186f / 255f);
		public static Color overheatColor = new Color(229f / 255f, 126f / 255f, 36f/255f, 186f / 255f);
	}
}
