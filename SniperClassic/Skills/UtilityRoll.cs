using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class CombatRoll : EntityStates.Commando.DodgeState
    {
        public override void OnEnter()
        {
			this.initialSpeedCoefficient = 5f;
			this.finalSpeedCoefficient = 2.5f;
			this.duration = 0.5f;
            base.OnEnter();

            if (base.skillLocator)
            {
				if (base.skillLocator && base.skillLocator.primary)
				{
					EntityStateMachine stateMachine = skillLocator.primary.stateMachine;
					if (stateMachine)
					{
						ReloadSnipe reloadSnipe = stateMachine.state as ReloadSnipe;
						if (reloadSnipe != null)
						{
							reloadSnipe.AutoReload();
						}
						else
                        {
							Snipe snipe = stateMachine.state as Snipe;
							if (snipe != null)
							{
								snipe.AutoReload();
							}
							else
                            {
								SniperClassic.ReloadController rc = base.gameObject.GetComponent<SniperClassic.ReloadController>();
								if (rc && rc.GetReloadQuality() != SniperClassic.ReloadController.ReloadQuality.Perfect)
                                {
									rc.SetReloadQuality(SniperClassic.ReloadController.ReloadQuality.Perfect);
								}
                            }
						}
					}
				}
			}
        }
    }
}
