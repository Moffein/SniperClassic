using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SniperClassic.Modules
{
    //based on https://github.com/ArcPh1r3/HenryMod/blob/master/HenryMod/Modules/ContentPacks.cs
    public class SniperContent
    {
        internal static ContentPack contentPack;

        public static AssetBundle assetBundle;

        public static BuffDef spotterStatDebuff;
        public static BuffDef spotterBuff;
        public static BuffDef spotterCooldownBuff;

        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<Type> entityStates = new List<Type>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();

        public static void CreateContentPack()
        {
            IL.RoR2.BuffCatalog.Init += FixBuffCatalog;
            contentPack = new ContentPack()
            {
                artifactDefs = new ArtifactDef[0],
                bodyPrefabs = bodyPrefabs.ToArray(),
                buffDefs = buffDefs.ToArray(),
                effectDefs = effectDefs.ToArray(),
                eliteDefs = new EliteDef[0],
                entityStateConfigurations = new EntityStateConfiguration[0],
                entityStateTypes = entityStates.ToArray(),
                equipmentDefs = new EquipmentDef[0],
                gameEndingDefs = new GameEndingDef[0],
                gameModePrefabs = new Run[0],
                itemDefs = new ItemDef[0],
                masterPrefabs = masterPrefabs.ToArray(),
                musicTrackDefs = new MusicTrackDef[0],
                networkedObjectPrefabs = new GameObject[0],
                networkSoundEventDefs = new NetworkSoundEventDef[0],
                projectilePrefabs = projectilePrefabs.ToArray(),
                sceneDefs = new SceneDef[0],
                skillDefs = skillDefs.ToArray(),
                skillFamilies = skillFamilies.ToArray(),
                surfaceDefs = new SurfaceDef[0],
                survivorDefs = survivorDefs.ToArray(),
                unlockableDefs = new UnlockableDef[0]
            };

            On.RoR2.ContentManager.SetContentPacks += AddContent;
        }

        private static void AddContent(On.RoR2.ContentManager.orig_SetContentPacks orig, List<ContentPack> newContentPacks)
        {
            newContentPacks.Add(contentPack);
            orig(newContentPacks);
        }

        //https://github.com/ArcPh1r3/HenryMod/blob/master/HenryMod/Modules/Buffs.cs
        //Remove this when this is fixed
        internal static void FixBuffCatalog(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.Next.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.buffDefs)))
            {
                return;
            }

            c.Remove();
            c.Emit(OpCodes.Ldsfld, typeof(ContentManager).GetField(nameof(ContentManager.buffDefs)));
        }
    }
}
