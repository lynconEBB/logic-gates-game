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

        private List<WireConnection> _socketedConnection = new();
        
        [SerializeField]
        private Color invalidColor = Color.red;
        [SerializeField]
        private Color validColor = Color.green;
        [SerializeField]
        private Color defaultColor = Color.white;

        private const float positionTolerance = 0.001f;
        
        private NodeComponent _nodeComponent;
        private Material _material;
        
        protected override void Awake()
        {
            base.Awake();
            _nodeComponent = GetComponentInParent<NodeComponent>();
        }

        protected override void Start()
        {
            base.Start();
            _material = meshRenderer.material;
            _material.color = defaultColor;
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactableObject is WireConnection wireConnection)
            {
                _material.color = _nodeComponent.CanConnect(wireConnection.GetOtherNode())
                    ? validColor
                    : invalidColor;
            }
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            _material.color = defaultColor; 
        }
        
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return interactable is WireConnection wireConnection 
                   && !_socketedConnection.Contains(wireConnection);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return !interactable.isSelected && interactable is WireConnection wireConnection && _nodeComponent.CanConnect(wireConnection.GetOtherNode())
                   && !_socketedConnection.Contains(wireConnection);         
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
                };
                
                wireConnection.CurrentNodeId = _nodeComponent.NodeId;
                _nodeComponent.ConnectTo(wireConnection.GetOtherNode());
            }
        }
        
    }
}