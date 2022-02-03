using RoR2;
using UnityEngine.Networking;

namespace SniperClassic.Modules.Achievements {
    public class CharacterUnlockAchievement : GenericModdedUnlockable {

        public static string TestAchievementIncrement = "3";
        public override string AchievementTokenPrefix => TestAchievementIncrement + "SNIPERCLASSIC_CHARACTER";
        public override string AchievementSpriteName => "texsniperUnlock";
        public override string PrerequisiteUnlockableIdentifier => "";

        public Inventory droneInventory;

        public override void OnInstall() {
            base.OnInstall();
            MinionOwnership.onMinionGroupChangedGlobal += MinionOwnership_onMinionGroupChangedGlobal;
        }

        public override void OnUninstall() {
            MinionOwnership.onMinionGroupChangedGlobal -= MinionOwnership_onMinionGroupChangedGlobal;
            base.OnUninstall();
        }

        private void MinionOwnership_onMinionGroupChangedGlobal(MinionOwnership minion) {

            CharacterMaster playerMaster = base.localUser.cachedMasterController.master;
            if (!playerMaster)
                return;
            //todo: come on I thought this was foolproof
            UnityEngine.Debug.LogWarning(minion.GetComponent<Deployable>() != null);
            if (minion.GetComponent<Deployable>())
                return;

            //A: giving another minion a scanner will work fine, but I don't know what that case may be,
                //so I'll leave it open cause I imagine it would still be fitting
                //if goobo jr is not a deployable then yea
                    //however you'd need both a radarscanner and goobojr as equipments so might not be so bad
                        //so yea if there's a mulT and he think this far ahead, he deserves it
            //B: yes, there is a much less retarded way to check characterbody, but dang look how simple that is
            //if (minion.gameObject.name != "EquipmentDroneMaster(Clone)")
            //    return;

            Inventory minionInventory = minion.GetComponent<Inventory>();
            NetworkInstanceId netId = playerMaster.netId;

            if (minionInventory && minion.group != null && (minion.group.ownerId == netId)) {

                droneInventory = minionInventory;
                minionInventory.onInventoryChanged += CharacterUnlockAchievement_onInventoryChanged;
            }
        }

        private void CharacterUnlockAchievement_onInventoryChanged() {

            if (droneInventory.currentEquipmentIndex == RoR2Content.Equipment.Scanner.equipmentIndex) {
                base.Grant();
                droneInventory.onInventoryChanged -= CharacterUnlockAchievement_onInventoryChanged;
            }
        }
    }
}