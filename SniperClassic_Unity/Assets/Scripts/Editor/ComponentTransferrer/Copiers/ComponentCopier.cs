using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    public abstract class ComponentCopierBase
    { /*uh*/

        public abstract bool hasComponent { get; }

        public virtual string copyReport { get; set; }
        public virtual string pasteReport { get; set; }

        public abstract void StoreComponent(GameObject copiedObject);

        public abstract void PasteComponent(GameObject selected, List<Component> remainingComponents);

        public void TransferAllComponents(GameObject from, GameObject to)
        {
            StoreComponent(from);
            PasteComponent(to, null);
        }

        protected void PasteAllComponents(GameObject selected, List<Component> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(components[i]);

                System.Type componentType = components[i].GetType();

                Component newComponent = selected.GetComponent(componentType);
                if (newComponent == null)
                {
                    newComponent = selected.AddComponent(componentType);
                    Undo.RegisterCreatedObjectUndo(newComponent, "paste copied components");

                    pasteReport += $"\nadded {componentType}";
                }
                else
                {
                    //update values of existing component. doesn't support multiple of same component.
                    pasteReport += $"\nalready had {componentType}. updating values. doesn't support multiple of same component";
                }
                UnityEditorInternal.ComponentUtility.PasteComponentValues(newComponent);
            }
        }
    }

    public abstract class ComponentCopier<T> : ComponentCopierBase where T : Component
    {

        public override bool hasComponent => storedComponent != null;

        public T storedComponent;

        public override void StoreComponent(GameObject copiedObject)
        {

            copyReport = "";
            storedComponent = copiedObject.GetComponent<T>();
            copyReport = $"{storedComponent?.GetType().ToString()}, ";
        }
        public override void PasteComponent(GameObject selected, List<Component> remainingComponents)
        {
            if (!hasComponent)
                return;

            pasteReport = "";
            Undo.RegisterCompleteObjectUndo(selected, "paste copied components");

            HandlePastedComponent(selected, remainingComponents);
            storedComponent = null;
        }

        protected abstract void HandlePastedComponent(GameObject selected, List<Component> remainingComponents);

        protected T GetOrAddPastedComponent<C>(GameObject selected) where C : Component
        {

            T newComponent = selected.GetComponent<T>();
            if (newComponent == null)
            {
                newComponent = selected.AddComponent<T>();
            }
            else
            {
                pasteReport += $"\nalready had {typeof(T)}";
            }
            Undo.RegisterCreatedObjectUndo(newComponent, "paste copied components");

            return newComponent;
        }

        protected void RemoveStoredComponentFromRemaining(List<Component> remainingComponents)
        {
            Debug.LogWarning($"removing {storedComponent} from {remainingComponents.Count} remaining components");
            remainingComponents.Remove(storedComponent);
        }
    }
}