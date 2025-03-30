using RoR2;
using RoR2.Skills;
using SniperClassic.Modules;

namespace SniperClassic.Hooks
{
    internal class SpotterRechargeCooldown
    {
        internal SpotterRechargeCooldown()
        {
            if (SpotterRechargeController.scaleWithAttackSpeed) return;

            On.RoR2.SkillLocator.ApplyAmmoPack += SkillLocator_ApplyAmmoPack;
            On.RoR2.GenericSkill.ApplyAmmoPack += GenericSkill_ApplyAmmoPack;
            On.RoR2.SkillLocator.DeductCooldownFromAllSkillsAuthority += SkillLocator_DeductCooldownFromAllSkillsAuthority;
        }

        private void SkillLocator_ApplyAmmoPack(On.RoR2.SkillLocator.orig_ApplyAmmoPack orig, SkillLocator self)
        {
            orig(self);
            if (self.special && self.special.defaultSkillDef && self.special.defaultSkillDef.activationStateMachineName == "DroneLauncher") { }
            {
                UnityEngine.Debug.Log("ApplyAmmoPack skillLocator");
                SpotterRechargeController src = self.GetComponent<SpotterRechargeController>();
                if (src) src.ResetSpotterCooldownServer();
            }
        }

        private void SkillLocator_DeductCooldownFromAllSkillsAuthority(On.RoR2.SkillLocator.orig_DeductCooldownFromAllSkillsAuthority orig, SkillLocator self, float deduction)
        {
            orig(self, deduction);
            if (self.special && self.special.defaultSkillDef && self.special.defaultSkillDef.activationStateMachineName == "DroneLauncher") { }
            {
                UnityEngine.Debug.Log("DeductCooldown");
                SpotterRechargeController src = self.GetComponent<SpotterRechargeController>();
                if (src) src.DeductSpotterCooldownServer(deduction);
            }
        }

        //This type of matching is bad
        private void GenericSkill_ApplyAmmoPack(On.RoR2.GenericSkill.orig_ApplyAmmoPack orig, RoR2.GenericSkill self)
        {
            orig(self);

            if (self.characterBody && self.skillFamily && self.skillFamily.defaultSkillDef && self.skillFamily.defaultSkillDef.activationStateMachineName == "DroneLauncher")
            {
                UnityEngine.Debug.Log("ApplyAmmoPack genericskill");
                SpotterRechargeController src = self.characterBody.GetComponent<SpotterRechargeController>();
                if (src) src.ResetSpotterCooldownServer();
            }
        }
    }
}
