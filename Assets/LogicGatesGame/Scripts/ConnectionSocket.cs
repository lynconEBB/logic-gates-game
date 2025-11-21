using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class ConnectionSocket : XRProximityInteractor
    {
        [SerializeField] 
        private float debugRadius = 0.1f;
        private NodeComponent _nodeComponent;
        
        protected override void Awake()
        {
            base.Awake();
            _nodeComponent = GetComponentInParent<NodeComponent>();
        }

        public Node GetOwningNode()
        {
            return _nodeComponent.Node;
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return interactable is WireConnection && !interactable.isSelected &&
                     interactable.transform.position != GetAttachTransform(interactable).position;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactableObject is WireConnection wireConnection)
            {
                wireConnection.transform.position = GetAttachTransform(args.interactableObject).position;
            }
        }
    }
}