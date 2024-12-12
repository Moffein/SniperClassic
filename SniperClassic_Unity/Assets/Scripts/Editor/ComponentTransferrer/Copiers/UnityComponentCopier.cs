using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    //handles unity components such as colliders rigidbodies and joints
    //you know just 3 random examples
    public class UnityComponentCopier : ComponentCopier<Component>
    {
        public override bool hasComponent => storedUnityComponents != null || storedUnityComponents?.Count == 0;

        public List<Component> storedUnityComponents;

        public override void StoreComponent(GameObject copiedObject)
        {
            copyReport = "";
            storedUnityComponents = copiedObject.GetComponents<Component>().ToList();

            storedUnityComponents.RemoveAll(comp => comp is MonoBehaviour || comp is Transform);

            for (int i = 0; i < storedUnityComponents.Count; i++)
            {
                copyReport += $"{storedUnityComponents[i].GetType().ToString()}, ";
            }
        }

        public override void PasteComponent(GameObject selected, List<Component> remainingComponents)
        {
            base.PasteComponent(selected, remainingComponents);

            storedUnityComponents = null;
        }

        protected override void HandlePastedComponent(GameObject selected, List<Component> remainingComponents)
        {
            PasteAllComponents(selected, storedUnityComponents);
        }
    }
}

