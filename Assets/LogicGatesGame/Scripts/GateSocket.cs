using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class GateSocket : XRSocketInteractor
    {
        private CircuitController _circuitController;
        
        public CircuitController CircuitController => _circuitController;

        protected override void Awake()
        {
            base.Awake();
            
            _circuitController = GetComponentInParent<CircuitController>();
        }

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable)
                   && (interactablesHovered.Count == 0 || interactable == interactablesHovered[0]);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            TelemetryManager.Instance?.Increment(CircuitTelemetryManager.KeyGates);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            TelemetryManager.Instance?.Decrement(CircuitTelemetryManager.KeyGates);
        }
    }
}