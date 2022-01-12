using System;
using UnityEngine;
using R2API;
using RoR2;
using R2API.Utils;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SniperClassic.Modules.Achievements;
using static R2API.LoadoutAPI;

namespace SniperClassic.Modules
{
    public static class SniperSkins
    {

        public static void RegisterSkins()
        {

            #region LanguageTokens
            LanguageAPI.Add("SNIPERBODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("SNIPERBODY_MASTERY_SKIN_NAME", "Spec Ops");
            LanguageAPI.Add("SNIPERBODY_TYPHOON_SKIN_NAME", "Wasteland");
            #endregion

            GameObject bodyPrefab = SniperClassic.SniperBody;
            GameObject modelTransform = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            ModelSkinController skinController = modelTransform.AddComponent<ModelSkinController>();
            ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            List<SkinDef> skinDefs = new List<SkinDef>();

            #region GameobjectActivation
            GameObject beret = childLocator.FindChildGameObject("BeretModel");
            GameObject[] activatableGameObjects = new GameObject[] { beret, };

            SkinDef.GameObjectActivation[] getGameObjectActivations(params GameObject[] activatedObjects)
            {
                List<SkinDef.GameObjectActivation> GameObjectActivations = new List<SkinDef.GameObjectActivation>();

                for (int i = 0; i < activatableGameObjects.Length; i++)
                {
                    bool activate = activatedObjects.Contains(activatableGameObjects[i]);

                    if (activatableGameObjects[i] == beret)
                    {
                        switch (Config.beret)
                        {
                            case Config.Beret.True:
                                break;
                            case Config.Beret.False:
                                activate = false;
                                break;
                            case Config.Beret.AllSkins:
                                activate = true;
                                break;
                        }
                    }

                    GameObjectActivations.Add(new SkinDef.GameObjectActivation
                    {
                        gameObject = activatableGameObjects[i],
                        shouldActivate = activate
                    });
                }

                return GameObjectActivations.ToArray();
            }
            #endregion

            #region default
            SkinDefInfo defaultSkinDefInfo = new SkinDefInfo();
            defaultSkinDefInfo.Name = "SNIPERBODY_DEFAULT_SKIN_NAME";
            defaultSkinDefInfo.NameToken = "SNIPERBODY_DEFAULT_SKIN_NAME";         //actual skin icon coming soon
            defaultSkinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f / 255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), new Color(78f / 255f, 80f / 255f, 111f / 255f)); //SniperContent.assetBundle.LoadAsset<Sprite>("texEnforcerAchievement");
            defaultSkinDefInfo.RootObject = modelTransform;

            defaultSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            defaultSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            defaultSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            defaultSkinDefInfo.GameObjectActivations = getGameObjectActivations();

                                                  //enter a list of strings in order of rendererinfos
                                                  //null or empty strings simply skips mesh replacement
            defaultSkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                "meshSniperDefault_Gun",
                "meshSniperDefault_GunAlt",
                "meshSniperDefault_CriticalHitCarl",
                "meshSniperMastery_Beret",
                "meshSniperDefault"
                );

            defaultSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;

            SkinDef defaultSkinDef = Skins.CreateSkinDef(defaultSkinDefInfo);
            skinDefs.Add(defaultSkinDef);

            #endregion

            #region Mastery
            SkinDefInfo masterySkinDefInfo = new SkinDefInfo();
            masterySkinDefInfo.Name = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masterySkinDefInfo.NameToken = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masterySkinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f / 255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), new Color(78f / 255f, 80f / 255f, 111f / 255f)); //SniperContent.assetBundle.LoadAsset<Sprite>("texSexforcerAchievement");
            masterySkinDefInfo.UnlockableDef = SniperUnlockables.MasteryUnlockableDef;
            masterySkinDefInfo.RootObject = modelTransform;

            masterySkinDefInfo.BaseSkins = new SkinDef[] { defaultSkinDef };
            masterySkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            masterySkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            masterySkinDefInfo.GameObjectActivations = getGameObjectActivations(beret);

            masterySkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                null,//"meshSniperDefault_Gun",
                null,//"meshSniperDefault_GunAlt",
                null,//"meshSniperDefault_CriticalHitCarl",
                null,//"meshSniperMastery_Beret",
                "meshSniperMastery"
                );

            masterySkinDefInfo.RendererInfos = new CharacterModel.RendererInfo[defaultSkinDef.rendererInfos.Length];
            defaultSkinDef.rendererInfos.CopyTo(masterySkinDefInfo.RendererInfos, 0);

            Material theyallusethesameonehuh = Assets.CreateMaterial("matSniperMastery", 1f, Color.white, 1f);
            masterySkinDefInfo.RendererInfos[0].defaultMaterial = theyallusethesameonehuh;
            masterySkinDefInfo.RendererInfos[1].defaultMaterial = theyallusethesameonehuh;
            masterySkinDefInfo.RendererInfos[2].defaultMaterial = theyallusethesameonehuh;
            masterySkinDefInfo.RendererInfos[3].defaultMaterial = theyallusethesameonehuh;
            masterySkinDefInfo.RendererInfos[4].defaultMaterial = theyallusethesameonehuh;

            SkinDef masterySkin = Skins.CreateSkinDef(masterySkinDefInfo);
            skinDefs.Add(masterySkin);
            #endregion

            #region MasteryAlt
            SkinDefInfo masteryAltSkinDefInfo = new SkinDefInfo();
            masteryAltSkinDefInfo.Name = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masteryAltSkinDefInfo.NameToken = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masteryAltSkinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f / 255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), new Color(78f / 255f, 80f / 255f, 111f / 255f)); //SniperContent.assetBundle.LoadAsset<Sprite>("texSexforcerAchievement");
            masteryAltSkinDefInfo.UnlockableDef = SniperUnlockables.MasteryUnlockableDef;
            masteryAltSkinDefInfo.RootObject = modelTransform;

            masteryAltSkinDefInfo.BaseSkins = new SkinDef[] { masterySkin };

            masteryAltSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            masteryAltSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            masteryAltSkinDefInfo.GameObjectActivations = getGameObjectActivations(beret);

            masteryAltSkinDefInfo.MeshReplacements = masterySkinDefInfo.MeshReplacements;

            masteryAltSkinDefInfo.RendererInfos = defaultSkinDef.rendererInfos;// new CharacterModel.RendererInfo[defaultSkinDef.rendererInfos.Length];
            //defaultSkinDef.rendererInfos.CopyTo(masteryAltSkinDefInfo.RendererInfos, 0);

            SkinDef masteryAltSkin = Skins.CreateSkinDef(masteryAltSkinDefInfo);
            if (Config.altMastery)
            {
                skinDefs.Add(masteryAltSkin);
            }
            #endregion

            #region Grandmastery
            SkinDefInfo grandmasterySkinDefInfo = new SkinDefInfo();
            grandmasterySkinDefInfo.Name = "SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME";
            grandmasterySkinDefInfo.NameToken = "SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME";
            grandmasterySkinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(38f / 255f, 56f / 255f, 92f / 255f), new Color(250f / 255f, 190f / 255f, 246f / 255f), new Color(106f / 255f, 98f / 255f, 104f / 255f), new Color(78f / 255f, 80f / 255f, 111f / 255f)); //SniperContent.assetBundle.LoadAsset<Sprite>("texSexforcerAchievement");
            grandmasterySkinDefInfo.UnlockableDef = SniperUnlockables.GrandMasteryUnlockableDef;
            grandmasterySkinDefInfo.RootObject = modelTransform;

            grandmasterySkinDefInfo.BaseSkins = new SkinDef[] { defaultSkinDef };
            grandmasterySkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            grandmasterySkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            grandmasterySkinDefInfo.GameObjectActivations = getGameObjectActivations();

            grandmasterySkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                "meshSniperQuentin_Gun",
                null,//"meshSniperDefault_GunAlt",
                "meshSniperQuentin_CriticalHitCarl",
                null,//"meshSniperMaster_Beret",
                "meshSniperQuentin"
                );;

            grandmasterySkinDefInfo.RendererInfos = new CharacterModel.RendererInfo[defaultSkinDef.rendererInfos.Length];
            defaultSkinDef.rendererInfos.CopyTo(grandmasterySkinDefInfo.RendererInfos, 0);

            Material grandmasteryMaterial = Assets.CreateMaterial("matSniperQuentin", 1f, Color.white, 1f);

            grandmasterySkinDefInfo.RendererInfos[0].defaultMaterial = grandmasteryMaterial;
            grandmasterySkinDefInfo.RendererInfos[1].defaultMaterial = theyallusethesameonehuh; //use mastery material for now cause it has orange
            grandmasterySkinDefInfo.RendererInfos[2].defaultMaterial = grandmasteryMaterial;
            grandmasterySkinDefInfo.RendererInfos[3].defaultMaterial = grandmasteryMaterial;
            grandmasterySkinDefInfo.RendererInfos[4].defaultMaterial = grandmasteryMaterial;

            SkinDef grandmasterySkin = Skins.CreateSkinDef(grandmasterySkinDefInfo);
            skinDefs.Add(grandmasterySkin);
            #endregion

            skinController.skins = skinDefs.ToArray();
        }
    }
}