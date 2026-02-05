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
    }
}