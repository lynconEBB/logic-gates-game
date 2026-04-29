using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public abstract class Gate : XRGrabInteractable
    {
        protected abstract NodeClass GateNodeClass { get; }
        protected abstract NodeComponent[] InputNodes { get; }
        protected abstract NodeComponent OutputNode { get; }

        private int? _gateNodeId;
        private CircuitController _circuitController;

        protected override void Awake()
        {
            base.Awake();
            SetNodeInteractablesEnabled(false);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is not GateSocket socket) return;

            if (!_gateNodeId.HasValue && socket.CircuitController != null)
                InitializeControllerNodes(socket.CircuitController);

            if (!_gateNodeId.HasValue)
                return;

            SetNodeInteractablesEnabled(true);
        }


        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            base.OnSelectExited(args);

            if (_circuitController == null || args.interactorObject is not GateSocket) return;

            SetNodeInteractablesEnabled(false);

            foreach (var input in InputNodes)
                foreach (var socket in input.GetComponentsInChildren<ConnectionSocket>())
                    socket.DisconnectAll();

            foreach (var socket in OutputNode.GetComponentsInChildren<ConnectionSocket>())
                socket.DisconnectAll();
        }

        private void InitializeControllerNodes(CircuitController circuitController)
        {
            _circuitController = circuitController;

            foreach (var input in InputNodes)
            {
                input.AssignController(_circuitController, NodeClass.GateInput);
                input.GetComponentInChildren<StateVisualizer>().SetNodeObserved(input.Node);
            }

            OutputNode.AssignController(_circuitController, NodeClass.Simple);
            OutputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(OutputNode.Node);

            _gateNodeId = _circuitController.AddNode(GateNodeClass);

            foreach (var input in InputNodes)
                _circuitController.ConnectNodes(_gateNodeId.Value, input.NodeId, automatic: true);

            _circuitController.ConnectNodes(OutputNode.NodeId, _gateNodeId.Value, automatic: true);

            var gateNodes = new List<Node>();
            foreach (var input in InputNodes) gateNodes.Add(input.Node);
            gateNodes.Add(OutputNode.Node);
            gateNodes.Add(_circuitController.GetNode(_gateNodeId.Value));

            foreach (var a in gateNodes)
                foreach (var b in gateNodes)
                    if (a != null && b != null && a != b)
                        a.AddToBlacklist(b.Id);
        }

        private void SetNodeInteractablesEnabled(bool enabled)
        {
            foreach (var input in InputNodes)
            {
                SetNodeInteractableEnable(input, enabled);
            }
            SetNodeInteractableEnable(OutputNode, enabled);
        }

        private void SetNodeInteractableEnable(NodeComponent node, bool isEnabled)
        {
            foreach (var socket in node.GetComponentsInChildren<ConnectionSocket>())
                socket.enabled = isEnabled;
            foreach (var initializer in node.GetComponentsInChildren<ConnectionInitializer>())
                initializer.enabled = isEnabled;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_circuitController == null || !_gateNodeId.HasValue)
                return;

            foreach (var input in InputNodes)
                RemoveControllerNode(input.NodeId);

            RemoveControllerNode(OutputNode.NodeId);
            RemoveControllerNode(_gateNodeId.Value);
        }

        private void RemoveControllerNode(int nodeId)
        {
            if (_circuitController.Nodes.ContainsKey(nodeId))
                _circuitController.RemoveNode(nodeId);
        }
    }
}
