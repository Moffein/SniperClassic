using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    public class UnhandledComponentsReporter : ComponentCopier<Component>
    {
        public override bool hasComponent => true;

        public override string copyReport { get => ""; }

        protected override void HandlePastedComponent(GameObject selected, List<Component> remainingComponents)
        {
            Debug.LogWarning(remainingComponents);
            string typesLog = "";
            for (int i = 0; i < remainingComponents.Count; i++)
            {
                typesLog += $"{remainingComponents[i].GetType()}, ";
            }

            if (!string.IsNullOrEmpty(typesLog))
            {
                pasteReport += $"\nfollowing components from original object are not supported: {typesLog}";
            }
        }
    }
}