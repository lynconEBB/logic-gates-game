using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class ConnectionSocket : XRProximityInteractor
    {
        [SerializeField] 
        private float debugRadius = 0.1f;
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Outline outline;
        [SerializeField]
        private Color invalidColor = Color.red;
        [SerializeField]
        private Color validColor = Color.green;

        private NodeComponent _nodeComponent;
        private List<WireConnection> _socketedConnection = new();
        
        protected override void Awake()
        {
            base.Awake();
            _nodeComponent = GetComponentInParent<NodeComponent>();
        }

        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            base.GetValidTargets(targets);
            targets.RemoveAll(t => t is WireConnection w && _socketedConnection.Contains(w));
        }

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return interactable is WireConnection && _nodeComponent.Node != null;
        }
        
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactableObject is WireConnection wireConnection)
            {
                outline.OutlineColor = _nodeComponent.CanConnect(wireConnection.GetOtherNode()?.Id)
                    ? validColor
                    : invalidColor;
                outline.enabled = true;
            }
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            outline.enabled = false;            
        }
        

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return !interactable.isSelected 
                   && interactable is WireConnection wireConnection
                   && _nodeComponent.CanConnect(wireConnection.GetOtherNode()?.Id);

        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactableObject is WireConnection wireConnection)
            {
                wireConnection.transform.position = GetAttachTransform(args.interactableObject).position;
                _socketedConnection.Add(wireConnection);
                wireConnection.OnDestroyed += () =>
                {
                    _socketedConnection.Remove(wireConnection);
                    _nodeComponent.DisconnectFrom(wireConnection.GetOtherNode()?.Id);
                };
                
                wireConnection.CurrentNode = _nodeComponent.Node;
                _nodeComponent.ConnectTo(wireConnection.GetOtherNode()?.Id);
            }
        }
    }
}