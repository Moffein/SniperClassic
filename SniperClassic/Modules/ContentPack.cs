using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using R2API;
using UnityEngine.AddressableAssets;

namespace SniperClassic.Modules
{
    public class SniperContent : IContentPackProvider
    {
        internal static ContentPack contentPack = new ContentPack();

        public static DamageAPI.ModdedDamageType SpotterDebuffOnHit;
        public static DamageAPI.ModdedDamageType Shock5sNoDamage;
        public static DamageAPI.ModdedDamageType SniperClassicDamage;

        public static AssetBundle assetBundle;

        //Decouple from Spotter?
        public static BuffDef spotterStatDebuff;

        //These show up on the enemy
        public static BuffDef spotterBuff;
        public static BuffDef spotterScepterBuff;
        public static BuffDef spotterCooldownBuff;

        //These show up on the Sniper
        public static BuffDef spotterPlayerCooldownBuff;
        public static BuffDef spotterPlayerReadyBuff;


        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<Type> entityStates = new List<Type>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();
        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();

        public string identifier => "SniperClassic.content";

        public static class Skills
        {
            public static class Primary
            {
                public static SkillDef Snipe;
                public static SkillDef Mark;
                public static SkillDef HardImpact;
            }

            public static class Secondary
            {
                public static SkillDef SteadyAim;
            }

            public static class Utility
            {
                public static SkillDef Backflip;
                public static SkillDef Roll;
            }

            public static class Special
            {
                public static SkillDef Feedback;
                public static SkillDef Disrupt;
                public static SkillDef FeedbackScepter;
                public static SkillDef DisruptScepter;
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            CreateBuffs();
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());
            contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.networkedObjectPrefabs.Add(networkedObjectPrefabs.ToArray());

            for (int i = 0; i < skillDefs.Count; i++) {
                SkillDef skillDef = skillDefs[i];
                if (string.IsNullOrEmpty((skillDef as ScriptableObject).name)) {
                    (skillDef as ScriptableObject).name = skillDef.skillName;
                }
            }

            contentPack.skillDefs.Add(skillDefs.ToArray());

            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.survivorDefs.Add(survivorDefs.ToArray());
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void FixScriptableObjectName(BuffDef buff)
        {
            (buff as ScriptableObject).name = buff.name;
        }

        public void CreateBuffs()
        {
            BuffDef spotterDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterDef.buffColor = new Color(1f, 1f, 1f);
            spotterDef.canStack = false;
            spotterDef.isDebuff = false;
            spotterDef.name = "SniperClassicSpotted";
            spotterDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterReady.png");
            FixScriptableObjectName(spotterDef);
            SniperContent.buffDefs.Add(spotterDef);
            SniperContent.spotterBuff = spotterDef;

            BuffDef spotterScepterDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterScepterDef.buffColor = new Color(1f, 0f, 1f);
            spotterScepterDef.canStack = false;
            spotterScepterDef.isDebuff = false;
            spotterScepterDef.name = "SniperClassicSpottedScepter";
            spotterScepterDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterReady.png");
            FixScriptableObjectName(spotterScepterDef);
            SniperContent.buffDefs.Add(spotterScepterDef);
            SniperContent.spotterScepterBuff = spotterScepterDef;

            BuffDef spotterCooldownDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterCooldownDef.buffColor = new Color(1f, 1f, 1f);
            spotterCooldownDef.canStack = true;
            spotterCooldownDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterCooldown.png");
            spotterCooldownDef.isDebuff = false;
            spotterCooldownDef.name = "SniperClassicSpottedCooldown";
            FixScriptableObjectName(spotterCooldownDef);
            SniperContent.buffDefs.Add(spotterCooldownDef);
            SniperContent.spotterCooldownBuff = spotterCooldownDef;

            BuffDef spotterStatDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterStatDebuffDef.buffColor = new Color(0.8392157f, 0.227450982f, 0.227450982f);
            spotterStatDebuffDef.canStack = false;
            spotterStatDebuffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Treebot/bdWeak.asset").WaitForCompletion().iconSprite;
            spotterStatDebuffDef.isDebuff = true;
            spotterStatDebuffDef.name = "SniperClassicSpottedStatDebuff";
            FixScriptableObjectName(spotterStatDebuffDef);
            SniperContent.buffDefs.Add(spotterStatDebuffDef);
            SniperContent.spotterStatDebuff = spotterStatDebuffDef;

            BuffDef spotterPlayerReadyDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterPlayerReadyDef.buffColor = new Color(1f, 1f, 1f);
            spotterPlayerReadyDef.canStack = false;
            spotterPlayerReadyDef.isDebuff = false;
            spotterPlayerReadyDef.name = "SniperClassicSpotterPlayerReady";
            spotterPlayerReadyDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterReady.png");
            FixScriptableObjectName(spotterPlayerReadyDef);
            SniperContent.buffDefs.Add(spotterPlayerReadyDef);
            SniperContent.spotterPlayerReadyBuff = spotterPlayerReadyDef;

            BuffDef spotterPlayerCooldownDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterPlayerCooldownDef.buffColor = new Color(1f,1f,1f);
            spotterPlayerCooldownDef.canStack = true;
            spotterPlayerCooldownDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterCooldown.png");
            spotterPlayerCooldownDef.isCooldown = true;
            spotterPlayerCooldownDef.isDebuff = false;
            spotterPlayerCooldownDef.name = "SniperClassicSpotterPlayerCooldown";
            FixScriptableObjectName(spotterPlayerCooldownDef);
            SniperContent.buffDefs.Add(spotterPlayerCooldownDef);
            SniperContent.spotterPlayerCooldownBuff = spotterPlayerCooldownDef;
        }
    }
}
