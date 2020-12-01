using EntityStates.SniperClassicSkills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SniperClassic
{
    public class ScopeController : MonoBehaviour
    {
        public void ResetCharge()
        {
            charge = 0f;
        }

        public void AddCharge(float f)
        {
            if (charge < 1f && !pauseCharge && characterBody.skillLocator.secondary.stock > 0)
            {
                charge += f;
                if (charge >= 1f)
                {
                    charge = 1f;
                    Util.PlaySound(ScopeController.fullChargeSoundString, base.gameObject);
                }
            }
        }

        public float ShotFired()
        {
            float toReturn = 0f;
            if (scoped)
            {
                
                if (charge > 0f && characterBody.skillLocator && characterBody.skillLocator.secondary.stock > 0)
                {
                    characterBody.skillLocator.secondary.stock--;
                    toReturn = charge;
                }
            }
            ResetCharge();
            return toReturn;
        }

        public void EnterScope()
        {
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = false;
            }
            scoped = true;
        }

        public void ExitScope()
        {
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = true;
            }
            scoped = false;
        }

        public void FixedUpdate()
        {
            if (characterBody && characterBody.skillLocator)
            {
                if(characterBody.skillLocator.secondary.stock < 1)
                {
                    charge = 0f;
                    characterBody.skillLocator.secondary.enabled = true;
                }
                else if (scoped)
                {
                    characterBody.skillLocator.secondary.enabled = false;
                }
            }

            if (!scoped && charge > 0f)
            {
                charge -= Time.fixedDeltaTime / ScopeController.chargeDecayDuration;
                if (charge < 0f)
                {
                    charge = 0f;
                }
            }
        }

        public bool IsScoped()
        {
            return scoped;
        }

        public void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
        }

        public bool pauseCharge = false;
        private bool scoped = false;
        public float charge = 0f;
        public float storedFOV = SecondaryScope.zoomFOV;
        CharacterBody characterBody;
        public static string fullChargeSoundString = "Play_MULT_m1_snipe_charge_end";
        public static float maxChargeMult = 4.0f;
        public static float chargeDecayDuration = 2f;
    }
}
