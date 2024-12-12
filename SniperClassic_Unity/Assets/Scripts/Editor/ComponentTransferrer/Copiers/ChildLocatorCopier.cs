using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    public class ChildLocatorCopier : ComponentCopier<ChildLocator>
    {

        protected override void HandlePastedComponent(GameObject selected, List<Component> remainingComponents)
        {
            ChildLocator newLocator = GetOrAddPastedComponent<ChildLocator>(selected);

            List<Transform> newChildren = selected.GetComponentsInChildren<Transform>(true).ToList();

            newLocator.transformPairs = new ChildLocator.NameTransformPair[storedComponent.transformPairs.Length];

            for (int i = 0; i < newLocator.transformPairs.Length; i++)
            {

                ChildLocator.NameTransformPair newPair = newLocator.transformPairs[i];
                ChildLocator.NameTransformPair storedPair = storedComponent.transformPairs[i];

                newPair.name = storedPair.name;

                if (storedPair.transform != null)
                {
                    //check all children for name that matches old transform
                    newPair.transform = newChildren.Find(tran => tran.name == storedPair.transform.name);
                }

                if (newPair.transform == null)
                {
                    pasteReport += $"\ncould not find child transform for {newPair.name}";
                }

                newLocator.transformPairs[i] = newPair;
            }

            RemoveStoredComponentFromRemaining(remainingComponents);
        }
    }
}