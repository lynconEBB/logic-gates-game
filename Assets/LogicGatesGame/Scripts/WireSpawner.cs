using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class WireSpawner : XRSimpleInteractable
    {
        public WireConnection wireConnection;
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            interactionManager.SelectCancel(args.interactorObject, this);
            interactionManager.SelectEnter(args.interactorObject, wireConnection);
        }
    }
}