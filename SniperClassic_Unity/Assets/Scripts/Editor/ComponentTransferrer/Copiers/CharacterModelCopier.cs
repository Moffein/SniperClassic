using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HenryTools.Editor
{
    public class CharacterModelCopier : ComponentCopier<CharacterModel>
    {
        protected override void HandlePastedComponent(GameObject selected, List<Component> remainingComponents)
        {
            CharacterModel newCharacterModel = GetOrAddPastedComponent<CharacterModel>(selected);

            newCharacterModel.baseRendererInfos = new CharacterModel.RendererInfo[storedComponent.baseRendererInfos.Length];

            List<Renderer> newRenderers = selected.GetComponentsInChildren<Renderer>(true).ToList();

            for (int i = 0; i < newCharacterModel.baseRendererInfos.Length; i++)
            {
                CharacterModel.RendererInfo newInfo = newCharacterModel.baseRendererInfos[i];
                CharacterModel.RendererInfo storedInfo = storedComponent.baseRendererInfos[i];

                string newRendererName = storedInfo.renderer.name;

                newInfo = storedInfo;
                newInfo.renderer = newRenderers.Find(rend => rend.name == newRendererName);
                if (newInfo.renderer == null)
                {
                    pasteReport += $"\n Could not find a renderer called {newRendererName} under the new GameObject";
                }

                newCharacterModel.baseRendererInfos[i] = newInfo;
            }

            RemoveStoredComponentFromRemaining(remainingComponents);
        }
    }
}
