using UnityEngine;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using SniperClassic.Modules;

namespace SniperClassic.Setup
{
    public static class BuildMaster
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            GameObject SniperMonsterMaster = R2API.PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "SniperClassicMonsterMaster", true);
            SniperContent.masterPrefabs.Add(SniperMonsterMaster);

            CharacterMaster cm = SniperMonsterMaster.GetComponent<CharacterMaster>();
            cm.bodyPrefab = SniperClassic.SniperBody;

            Component[] toDelete = SniperMonsterMaster.GetComponents<AISkillDriver>();
            foreach (AISkillDriver asd in toDelete)
            {
                UnityEngine.Object.Destroy(asd);
            }

            AISkillDriver roll = SniperMonsterMaster.AddComponent<AISkillDriver>();
            roll.skillSlot = SkillSlot.Utility;
            roll.requireSkillReady = true;
            roll.requireEquipmentReady = false;
            roll.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            roll.minDistance = 0f;
            roll.maxDistance = 30f;
            roll.selectionRequiresTargetLoS = false;
            roll.activationRequiresTargetLoS = false;
            roll.activationRequiresAimConfirmation = false;
            roll.movementType = AISkillDriver.MovementType.FleeMoveTarget;
            roll.aimType = AISkillDriver.AimType.MoveDirection;
            roll.ignoreNodeGraph = false;
            roll.driverUpdateTimerOverride = -1f;
            roll.noRepeat = true;
            roll.shouldSprint = true;
            roll.shouldFireEquipment = false;
            roll.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;

            AISkillDriver scopeAggressive = SniperMonsterMaster.AddComponent<AISkillDriver>();
            scopeAggressive.skillSlot = SkillSlot.Secondary;
            scopeAggressive.requireSkillReady = true;
            scopeAggressive.requireEquipmentReady = false;
            scopeAggressive.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scopeAggressive.minDistance = 30f;
            scopeAggressive.maxDistance = float.PositiveInfinity;
            scopeAggressive.selectionRequiresTargetLoS = true;
            scopeAggressive.activationRequiresTargetLoS = true;
            scopeAggressive.activationRequiresAimConfirmation = true;
            scopeAggressive.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            scopeAggressive.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scopeAggressive.ignoreNodeGraph = false;
            scopeAggressive.driverUpdateTimerOverride = 2f;
            scopeAggressive.noRepeat = true;
            scopeAggressive.shouldSprint = false;
            scopeAggressive.shouldFireEquipment = false;
            scopeAggressive.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            scopeAggressive.requiredSkill = SniperContent.Skills.Secondary.SteadyAim;

            AISkillDriver strafeShoot = SniperMonsterMaster.AddComponent<AISkillDriver>();
            strafeShoot.skillSlot = SkillSlot.Primary;
            strafeShoot.requireSkillReady = false;
            strafeShoot.requireEquipmentReady = false;
            strafeShoot.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeShoot.minDistance = 0f;
            strafeShoot.maxDistance = float.PositiveInfinity;
            strafeShoot.selectionRequiresTargetLoS = true;
            strafeShoot.activationRequiresTargetLoS = true;
            strafeShoot.activationRequiresAimConfirmation = true;
            strafeShoot.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeShoot.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            strafeShoot.ignoreNodeGraph = false;
            strafeShoot.noRepeat = false;
            strafeShoot.shouldSprint = false;
            strafeShoot.shouldFireEquipment = false;
            strafeShoot.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            strafeShoot.driverUpdateTimerOverride = 1f;

            AISkillDriver afk = SniperMonsterMaster.AddComponent<AISkillDriver>();
            afk.skillSlot = SkillSlot.None;
            afk.requireSkillReady = false;
            afk.requireEquipmentReady = false;
            afk.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            afk.minDistance = 0f;
            afk.maxDistance = float.PositiveInfinity;
            afk.selectionRequiresTargetLoS = false;
            afk.activationRequiresTargetLoS = false;
            afk.activationRequiresAimConfirmation = false;
            afk.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk.aimType = AISkillDriver.AimType.MoveDirection;
            afk.ignoreNodeGraph = false;
            afk.driverUpdateTimerOverride = -1f;
            afk.noRepeat = false;
            afk.shouldSprint = true;
            afk.shouldFireEquipment = false;
            afk.shouldTapButton = false;

            AISkillDriver afk2 = SniperMonsterMaster.AddComponent<AISkillDriver>();
            afk2.skillSlot = SkillSlot.None;
            afk2.requireSkillReady = false;
            afk2.requireEquipmentReady = false;
            afk2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            afk2.minDistance = 0f;
            afk2.maxDistance = float.PositiveInfinity;
            afk2.selectionRequiresTargetLoS = false;
            afk2.activationRequiresTargetLoS = false;
            afk2.activationRequiresAimConfirmation = false;
            afk2.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk2.aimType = AISkillDriver.AimType.MoveDirection;
            afk2.ignoreNodeGraph = false;
            afk2.driverUpdateTimerOverride = -1f;
            afk2.noRepeat = false;
            afk2.shouldSprint = true;
            afk2.shouldFireEquipment = false;
            afk2.shouldTapButton = false;
        }
    }
}
