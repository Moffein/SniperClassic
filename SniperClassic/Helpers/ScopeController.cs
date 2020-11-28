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
            fullyCharged = false;
        }

        public void AddCharge(float f)
        {
            if (!fullyCharged && !pauseCharge && characterBody.skillLocator.secondary.stock > 0)
            {
                charge += f;
                if (charge >= 1f)
                {
                    charge = 1f;
                    fullyCharged = true;
                    Util.PlaySound(ScopeController.fullChargeSoundString, base.gameObject);
                }
            }
        }

        public float ShotFired()
        {
            float toReturn = 0f;
            if (charge > 0f && characterBody.skillLocator && characterBody.skillLocator.secondary.stock > 0)
            {
                characterBody.skillLocator.secondary.stock--;
                toReturn = charge;
            }
            ResetCharge();
            return toReturn;
        }

        public void EnterScope()
        {
            ResetCharge();
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = false;
            }
            scoped = true;
        }

        public void ExitScope()
        {
            ResetCharge();
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
                if(characterBody.skillLocator.secondary.stock < 1 || !scoped)
                {
                    charge = 0f;
                    characterBody.skillLocator.secondary.enabled = true;
                }
                else
                {
                    characterBody.skillLocator.secondary.enabled = false;
                }
            }
        }

        public void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
        }

        private bool fullyCharged = false;
        public bool pauseCharge = false;
        private bool scoped = false;
        public float charge = 0f;
        CharacterBody characterBody;
        public static string fullChargeSoundString = "Play_MULT_m1_snipe_charge_end";
        public static float maxChargeMult = 4.0f;
    }
}
