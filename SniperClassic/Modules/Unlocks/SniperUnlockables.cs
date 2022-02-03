using R2API;
using RoR2;
using System;
using UnityEngine;

namespace SniperClassic.Modules.Achievements {

    public static class SniperUnlockables
    {

        public static UnlockableDef CharacterUnlockableDef;
        public static UnlockableDef MasteryUnlockableDef;
        public static UnlockableDef GrandMasteryUnlockableDef;

        public static void RegisterUnlockables()
        {
            CharacterUnlockableDef = Config.forceUnlock ? null : UnlockableAPI.AddUnlockable<CharacterUnlockAchievement>();// typeof(EnforcerUnlockAchievement.EnforcerUnlockAchievementServer));
            MasteryUnlockableDef = UnlockableAPI.AddUnlockable<MasteryAchievementButEpic>();
            GrandMasteryUnlockableDef = UnlockableAPI.AddUnlockable<GrandMasteryAchievement>();
        }
    }
    

    public abstract class GenericModdedUnlockable : ModdedUnlockable
    {
        public abstract string AchievementTokenPrefix { get; }
        public abstract string AchievementSpriteName { get; }

        public override string AchievementIdentifier { get => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_ID"; }
        public override string UnlockableIdentifier { get => AchievementTokenPrefix + "UNLOCKABLE_REWARD_ID"; }
        public override string AchievementNameToken { get => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_NAME"; }
        public override string AchievementDescToken { get => AchievementTokenPrefix + "UNLOCKABLE_ACHIEVEMENT_DESC"; }
        public override string UnlockableNameToken { get => AchievementTokenPrefix + "UNLOCKABLE_UNLOCKABLE_NAME"; }

        public override Sprite Sprite => SniperContent.assetBundle.LoadAsset<Sprite>(AchievementSpriteName);

        public override Func<string> GetHowToUnlock
        {
            get => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(AchievementNameToken),
                                Language.GetString(AchievementDescToken)
                            });
        }

        public override Func<string> GetUnlocked
        {
            get => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(AchievementNameToken),
                                Language.GetString(AchievementDescToken)
                            });
        }
    }

    public abstract class BaseMasteryUnlockable : GenericModdedUnlockable
    {
        public abstract string RequiredCharacterBody { get; }
        public abstract float RequiredDifficultyCoefficient { get; }
        //public CharacterBody RequiredCharacterBody { get; }

        //not sure if we use constructors
        //public GenericMasteryAchievement(float requiredDifficultyCoef, CharacterBody requiredCharBody)
        //{
        //    RequiredDifficultyCoefficient = requiredDifficultyCoef;
        //    RequiredCharacterBody = requiredCharBody;
        //}

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Run.onClientGameOverGlobal += OnClientGameOverGlobal;
        }
        public override void OnBodyRequirementBroken()
        {
            Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
            base.OnBodyRequirementBroken();
        }
        private void OnClientGameOverGlobal(Run run, RunReport runReport)
        {
            if ((bool)runReport.gameEnding && runReport.gameEnding.isWin)
            {
                DifficultyDef runDifficulty = DifficultyCatalog.GetDifficultyDef(runReport.ruleBook.FindDifficulty());
                if (runDifficulty.countsAsHardMode && runDifficulty.scalingValue >= RequiredDifficultyCoefficient)
                {
                    Grant();
                }
            }
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(RequiredCharacterBody);
        }
    }
}