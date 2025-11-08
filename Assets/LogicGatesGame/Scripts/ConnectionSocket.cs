using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class ConnectionSocket : XRProximityInteractor
    {
        [SerializeField] 
        private float debugRadius = 0.1f;
        
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return !interactable.isSelected &&
                     interactable.transform.position != GetAttachTransform(interactable).position;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            args.interactableObject.transform.position = GetAttachTransform(args.interactableObject).position;
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            string name = ((MonoBehaviour)args.interactableObject).name;
            Debug.Log("Entered: " + name);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            
            string name = ((MonoBehaviour)args.interactableObject).name;
            Debug.Log("Exited: " + name);
        }
    }
}