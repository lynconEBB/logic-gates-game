using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class ConnectionSocket : XRProximityInteractor
    {
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Outline outline;
        [SerializeField]
        private Color invalidColor = Color.red;
        [SerializeField]
        private Color validColor = Color.green;

        private NodeComponent _nodeComponent;
        public NodeComponent NodeComponent => _nodeComponent;
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
            return _nodeComponent.Node != null && interactable is WireConnection;
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
                   && wireConnection.NodeComponent == null
                   && _nodeComponent.CanConnect(wireConnection.GetOtherNode()?.Id);

        }

        public void DisconnectAll()
        {
            foreach (var wire in _socketedConnection.ToArray())
            {
                var wireInteractable = wire.GetComponentInParent<WireInteractable>();
                if (wireInteractable != null)
                    wireInteractable.AutoDestroy();
            }
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
                wireConnection.NodeComponent = _nodeComponent;
                _nodeComponent.ConnectTo(wireConnection.GetOtherNode()?.Id);
            }
        }
    }
}