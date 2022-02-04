using RoR2;
using UnityEngine.Networking;

namespace SniperClassic.Modules.Achievements {
    public class CharacterUnlockAchievement : GenericModdedUnlockable {

        public static string TestAchievementIncrement = "14";
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

            if (!minion.gameObject.name.Contains("EquipmentDrone"))
                return;

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