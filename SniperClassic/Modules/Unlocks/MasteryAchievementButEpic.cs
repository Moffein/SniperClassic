namespace SniperClassic.Modules.Achievements
{
    public class MasteryAchievementButEpic : BaseMasteryUnlockable
    {
        public override string AchievementTokenPrefix => "SNIPERCLASSIC_MASTERY";
        public override string PrerequisiteUnlockableIdentifier => "SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_ID";
        public override string AchievementSpriteName => "texSniperSkinMaster";

        public override string RequiredCharacterBody => "SniperClassicBody";

        public override float RequiredDifficultyCoefficient => 3f;
    }

    public class GrandMasteryAchievement : BaseMasteryUnlockable
    {
        public override string AchievementTokenPrefix => "SNIPERCLASSIC_GRANDMASTERY";
        public override string PrerequisiteUnlockableIdentifier => "SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_ID";
        public override string AchievementSpriteName => "texSniperSkinGrandmaster";

        public override string RequiredCharacterBody => "SniperClassicBody";

        public override float RequiredDifficultyCoefficient => 3.5f;
    }
}