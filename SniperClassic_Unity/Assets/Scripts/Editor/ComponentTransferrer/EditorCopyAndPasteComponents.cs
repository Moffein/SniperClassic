using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    public class EditorCopyAndPasteComponents
    {
        public static List<ComponentCopierBase> SupportedComponentCopiers = new List<ComponentCopierBase> {
            new ChildLocatorCopier(),
            new RagdollControllerCopier(),
            new CharacterModelCopier()
        };

        public static UnhandledComponentsReporter UnhandledComponentsReporter = new UnhandledComponentsReporter();
        public static RemainingComponentsCopier RemainingComponentsCopier = new RemainingComponentsCopier();

        public static List<Component> remainingComponents;

        [MenuItem("Tools/HenryTools/Record Components")]
        public static void CopyComponents()
        {
            string copyReport = "";

            GameObject selected = Selection.activeGameObject;

            for (int i = 0; i < SupportedComponentCopiers.Count; i++)
            {
                copyReport += RunCopierCopy(SupportedComponentCopiers[i], selected);
            }
            RunCopierCopy(UnhandledComponentsReporter, selected);
            RunCopierCopy(RemainingComponentsCopier, selected);

            remainingComponents = selected.GetComponents<Component>().ToList(); 

            if (string.IsNullOrEmpty(copyReport))
            {
                Debug.Log("did not copy any components", selected);
            }
            else
            {
                Debug.Log($"Note: This was intended for CharacterModels, and doesn't work with multiple of the same component.\nCopying from {selected.name}: {copyReport}\n", selected);
            }
        }

        private static string RunCopierCopy(ComponentCopierBase copier, GameObject selected)
        {
            copier.StoreComponent(selected);

            string addedReport = "";
            if (!string.IsNullOrEmpty(copier.copyReport))
            {
                addedReport = copier.copyReport;
            }

            return addedReport;
        }

        [MenuItem("Tools/HenryTools/Transfer Components (only supported components)")]
        public static void PasteSupportedComponents()
        {
            HandlePasteComponents(false);
        }

        [MenuItem("Tools/HenryTools/Transfer Components (include all components)")]
        public static void PasteAllComponents()
        {
            HandlePasteComponents(true);
        }

        private static void HandlePasteComponents(bool includeUnsupported)
        {
            string bigLog = "";

            GameObject selected = Selection.activeGameObject;

            for (int i = remainingComponents.Count - 1; i >= 0; i--)
            {
                if (remainingComponents[i] is Transform)
                {
                    remainingComponents.RemoveAt(i);
                }
            }

            for (int i = 0; i < SupportedComponentCopiers.Count; i++)
            {
                SupportedComponentCopiers[i].PasteComponent(selected, remainingComponents);
                bigLog += SupportedComponentCopiers[i].pasteReport;
            }

            if (includeUnsupported)
            {
                RemainingComponentsCopier.PasteComponent(selected, remainingComponents);
                bigLog += RemainingComponentsCopier.pasteReport;
            }
            else
            {
                UnhandledComponentsReporter.PasteComponent(selected, remainingComponents);
                bigLog += UnhandledComponentsReporter.pasteReport;
            }

            if (string.IsNullOrEmpty(bigLog))
            {
                Debug.Log("but nobody came.");
            }
            else
            {
                Debug.Log($"Paste Components Report for {selected}: {bigLog}\n", selected);
            }
        }
    }
}