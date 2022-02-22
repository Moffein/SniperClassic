using EntityStates;
using EntityStates.BeetleQueenMonster;
using EntityStates.GlobalSkills.LunarNeedle;
using EntityStates.SniperClassicSkills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SniperClassic
{
    public class ScopeController : NetworkBehaviour
    {
        public void ResetCharge()
        {
            charge = 0f;
        }

        public float GetMaxCharge()
        {
            return Mathf.Max(1f, 0.5f + 0.5f * characterBody.skillLocator.secondary.maxStock);
        }

        public float GetChargeMult(float currentCharge)
        {
            float maxCharge = GetMaxCharge();
            return Mathf.Lerp(1f, 1f + (ScopeController.maxChargeMult - 1f) * maxCharge, currentCharge / maxCharge);
        }

        public void AddCharge(float f)
        {
            float maxCharge = GetMaxCharge();
            if (charge < maxCharge && !pauseCharge && characterBody.skillLocator.secondary.stock > 0)
            {
                bool wasUncharged = charge < 1f;
                charge += f;
                if (wasUncharged && charge >= 1f)
                {
                    if (base.hasAuthority)
                    {
                        Util.PlaySound(ScopeController.fullChargeSoundString, base.gameObject);
                    }
                }

                if (charge > maxCharge)
                {
                    charge = maxCharge;
                }
            }
        }

        public float ShotFired(bool resetCharge = true)
        {
            float toReturn = 0f;
            if (scoped)
            {
                if (charge > 0f && characterBody.skillLocator && characterBody.skillLocator.secondary.stock > 0)
                {
                    //characterBody.skillLocator.secondary.stock--;
                    toReturn = charge;
                }
            }
            if (resetCharge)
            {
                ResetCharge();
            }
            return toReturn;
        }

        public void EnterScope()
        {
            UpdateRects();
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = false;
            }
            scoped = true;
            //animator.SetBool("scoped", true);
        }

        public void ExitScope()
        {
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = true;
            }
            scoped = false;
            //animator.SetBool("scoped", false);
        }

        public void FixedUpdate()
        {
            /*if (characterBody && characterBody.skillLocator)
            {
                if (characterBody.skillLocator.secondary.stock < 1)
                {
                    charge = 0f;
                    characterBody.skillLocator.secondary.enabled = true;
                }
                else if (scoped)
                {
                    if (charge < 1f)
                    {
                        characterBody.skillLocator.secondary.enabled = false;
                    }
                    else
                    {
                        characterBody.skillLocator.secondary.enabled = true;
                    }

                }
            }*/

            if (!scoped && charge > 0f)
            {
                charge -= Time.fixedDeltaTime / ScopeController.chargeDecayDuration;
                if (charge < 0f)
                {
                    charge = 0f;
                }
            }

            //Debug.Log(animator.GetFloat("SecondaryCharge") + " | ");
            animator.SetFloat("SecondaryCharge", charge);
            //Debug.Log(animator.GetFloat("SecondaryCharge"));

            if (this.hasAuthority)
            {
                bool chargeReady = scoped && charge > 0.2f;
                if (chargeReady != chargeShotReady)
                {
                    CmdSetChargeStatus(chargeReady);
                }
            }
        }
        
        /*private void ReplaceVisions()   //probably a bad way of doing this
        {
            if (characterBody.master && characterBody.master.inventory && characterBody.master.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0)
            {
                if (scoped)
                {
                    characterBody.skillLocator.primary.skillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.SniperClassicSkills.NeedleRifle));
                    characterBody.skillLocator.primary.skillDef.stockToConsume = 2;
                }
                else
                {
                    characterBody.skillLocator.primary.skillDef.activationState = new SerializableEntityStateType(typeof(FireLunarNeedle));
                    characterBody.skillLocator.primary.skillDef.stockToConsume = 1;
                }
            }
        }*/

        public void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
            healthComponent = characterBody.healthComponent;
            animator = characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            for (int i = 0; i < stockRects.Length; i++)
            {
                stockRects[i] = new Rect();
            }
        }

        private void UpdateRects()
        {
            float dimensions = Screen.height * 48f / 1080f;
            float originX = Screen.width / 2f - dimensions/2f - Screen.height * 228f/1080f;
            float originY = Screen.height / 2f + dimensions / 2f + Screen.height * 32f / 1080f;
            for (int i = 0; i < stockRects.Length; i++)
            {
                stockRects[i].width = dimensions;
                stockRects[i].height = dimensions;
                stockRects[i].position = new Vector2(originX + Screen.height * (i / maxStockPerRow) * 52f / 1080f, originY + Screen.height * (i % maxStockPerRow) * 12f / 1080f);
            }
        }

        private void OnGUI()
        {
            if (this.hasAuthority && scoped && !RoR2.PauseManager.isPaused && healthComponent && healthComponent.alive && storedFOV < SecondaryScope.maxFOV)
            {
                int totalStocks = characterBody.skillLocator.secondary.maxStock;
                if (totalStocks > stockRects.Length)
                {
                    totalStocks = stockRects.Length;
                }
                int currentStock = characterBody.skillLocator.secondary.stock;
                if (currentStock > totalStocks)
                {
                    currentStock = totalStocks;
                }

                for (int i = 0; i < totalStocks; i++)
                {
                    GUI.DrawTexture(stockRects[i], (i < currentStock) ? stockAvailable : stockEmpty, ScaleMode.StretchToFill, true, 0f);
                }
            }
        }

        [Command]
        private void CmdSetChargeStatus(bool value)    //solely used for sound syncing purposes
        {
            chargeShotReady = value;
        }

        public bool pauseCharge = false;

        private bool scoped = false;

        [SyncVar]
        public bool chargeShotReady = false;    //solely used for sound syncing purposes

        public bool IsScoped { get => scoped; }
        public float charge = 0f;
        public float storedFOV = SecondaryScope.zoomFOV;
        CharacterBody characterBody;
        HealthComponent healthComponent;
        private Animator animator;
        public static string fullChargeSoundString = "Play_SniperClassic_fullycharged";
        public static float chargeDecayDuration = 1.5f;
        public static float maxChargeMult = 3f;
        public static float chargeCircleScale = 1f;

        public static Texture2D stockEmpty;
        public static Texture2D stockAvailable;

        private Rect[] stockRects = new Rect[18];
        private int maxStockPerRow = 6;
    }
}
