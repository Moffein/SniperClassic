using System.Collections.Generic;
using UnityEngine;

namespace HenryTools.Editor
{
    public class RemainingComponentsCopier : UnhandledComponentsReporter
    {
        protected override void HandlePastedComponent(GameObject selected, List<Component> remainingComponents)
        {
            base.HandlePastedComponent(selected, remainingComponents);
            pasteReport += "\nTransferring anyways. There may be object references on the original components that are invalid on the new components.";

            PasteAllComponents(selected, remainingComponents);
        }
    }
}