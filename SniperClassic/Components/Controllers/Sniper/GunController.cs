using RoR2;
using UnityEngine;

namespace SniperClassic.Controllers
{
    public class GunController : MonoBehaviour
    {
        private enum EquippedGun
        {
            NONE = -1,
            DEFAULT,
            ALT,
            HEAVY
        }

        private CharacterBody characterBody;
        private CharacterModel model;
        private ChildLocator childLocator;
        private Material sniperMaterial;

        private EquippedGun currentGun = EquippedGun.NONE;

        private void Awake()
        {
            this.characterBody = this.gameObject.GetComponent<CharacterBody>();
            this.childLocator = this.gameObject.GetComponentInChildren<ChildLocator>();
            this.model = this.GetComponentInChildren<CharacterModel>();

            Invoke("CheckWeapon", 0.2f);
        }

        private void CheckWeapon()
        {
            //idk what the fuck this approach was
            // cache this, in case sniper ever swaps guns during a run
            //Material gunMat = this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial;
            //if (gunMat != null) this.sniperMaterial = gunMat;

            //string skillName = this.characterBody.skillLocator.primary.skillDef.skillNameToken;
            //switch (skillName)
            //{
            //    default:
            //        this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = sniperMaterial;
            //        this.childLocator.FindChild("AltRifle").gameObject.SetActive(false);
            //        break;
            //    case "SNIPERCLASSIC_PRIMARY_ALT_NAME":
            //        this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = null;
            //        this.childLocator.FindChild("AltRifle").gameObject.SetActive(true);
            //        break;
            //}

            EquippedGun checkGun;
            switch (this.characterBody.skillLocator.primary.skillDef.skillName)
            {
                case "Snipe":
                case "ReloadSnipe":
                default:
                    checkGun = EquippedGun.DEFAULT;
                    break;
                case "BattleRifle":
                case "ReloadBR":
                    checkGun = EquippedGun.ALT;
                    break;
                case "HeavySnipe":
                case "ReloadHeavySnipe":
                    checkGun = EquippedGun.HEAVY;
                    break;
            }

            if (currentGun == checkGun)
                return;

            currentGun = checkGun;

            this.childLocator.FindChildGameObject("GunModel").gameObject.SetActive(false);
            this.childLocator.FindChildGameObject("GunAltModel").gameObject.SetActive(false);

            switch (currentGun)
            {
                case EquippedGun.DEFAULT:
                    this.childLocator.FindChildGameObject("GunModel").gameObject.SetActive(true);
                    break;
                case EquippedGun.ALT:
                    this.childLocator.FindChildGameObject("GunAltModel").gameObject.SetActive(true);
                    break;
                case EquippedGun.HEAVY:
                    this.childLocator.FindChildGameObject("GunModel").gameObject.SetActive(true);
                    break;
            }
        }
    }
}