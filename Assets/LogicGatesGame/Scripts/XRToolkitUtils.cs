using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public static class XRToolkitUtils
    {
        public static int RemoveAllUnregistered(XRInteractionManager manager, List<IXRInteractable> interactables)
        {
            var numRemoved = 0;
            for (var i = interactables.Count - 1; i >= 0; --i)
            {
                if (!manager.IsRegistered(interactables[i]))
                {
                    interactables.RemoveAt(i);
                    ++numRemoved;
                }
            }

            return numRemoved;
        }
    }
}