using EntityStates.BeetleQueenMonster;
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

        public void AddCharge(float f)
        {
            if (charge < 1f && !pauseCharge && characterBody.skillLocator.secondary.stock > 0)
            {
                charge += f;
                if (charge >= 1f)
                {
                    charge = 1f;
                    if (base.hasAuthority)
                    {
                        Util.PlaySound(ScopeController.fullChargeSoundString, base.gameObject);
                    }
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
                    characterBody.skillLocator.secondary.stock--;
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
            animator.SetBool("scoped", true);
        }

        public void ExitScope()
        {
            if (characterBody && characterBody.skillLocator)
            {
                characterBody.skillLocator.secondary.enabled = true;
            }
            scoped = false;
            animator.SetBool("scoped", false);
        }

        public void FixedUpdate()
        {
            if (characterBody && characterBody.skillLocator)
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
                        animator.Play("SteadyAimCharge");
                    }
                    else
                    {
                        characterBody.skillLocator.secondary.enabled = true;
                    }

                    animator.SetFloat("SecondaryCharge", charge);
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

        public bool pauseCharge = false;

        private bool scoped = false;
        public bool IsScoped { get => scoped; }
        public float charge = 0f;
        public float storedFOV = SecondaryScope.zoomFOV;
        CharacterBody characterBody;
        HealthComponent healthComponent;
        private Animator animator;
        public static string fullChargeSoundString = "Play_SniperClassic_fullycharged";
        public static float chargeDecayDuration = 3f;

        public static float chargeCircleScale = 1f;

        public static Texture2D stockEmpty;
        public static Texture2D stockAvailable;

        private Rect[] stockRects = new Rect[18];
        private int maxStockPerRow = 6;
    }
}
