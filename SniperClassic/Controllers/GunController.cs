using RoR2;
using UnityEngine;

namespace SniperClassic.Controllers
{
    public class GunController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private CharacterModel model;
        private ChildLocator childLocator;
        private Material sniperMaterial;

        private void Awake()
        {
            this.characterBody = this.gameObject.GetComponent<CharacterBody>();
            this.childLocator = this.gameObject.GetComponentInChildren<ChildLocator>();
            this.model = this.GetComponentInChildren<CharacterModel>();

            Invoke("CheckWeapon", 0.2f);
        }

        private void CheckWeapon()
        {
            // cache this, in case sniper ever swaps guns during a run
            Material gunMat = this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial;
            if (gunMat != null) this.sniperMaterial = gunMat;

            string skillName = this.characterBody.skillLocator.primary.skillDef.skillNameToken;
            switch (skillName)
            {
                case "SNIPERCLASSIC_PRIMARY_ALT_NAME":
                    this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = sniperMaterial;
                    this.childLocator.FindChild("AltRifle").gameObject.SetActive(true);
                    break;
                default:
                    this.characterBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = null;
                    this.childLocator.FindChild("AltRifle").gameObject.SetActive(false);
                    break;
            }
        }
    }
}