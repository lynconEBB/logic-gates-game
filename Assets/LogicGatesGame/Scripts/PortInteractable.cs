using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class PortInteractable : XRSimpleInteractable
    {
        [SerializeField] 
        private Port port;
        private CircuitController _circuitController;

        protected override void Awake()
        {
            base.Awake();
            _circuitController = GetComponentInParent<CircuitController>();
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            Debug.Log("OnHoverEntered " + gameObject.name);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            Debug.Log("OnHoverExited " + gameObject.name);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            Debug.Log("OnSelectEntered " + gameObject.name);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            Debug.Log("OnSelectExited " + gameObject.name); 
        }
    }
}