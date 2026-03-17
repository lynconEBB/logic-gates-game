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

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is not GateSocket socket) return;

            _circuitController = socket.CircuitController;

            foreach (var input in InputNodes)
            {
                input.AssignController(_circuitController, NodeClass.Simple);
                input.GetComponentInChildren<StateVisualizer>().SetNodeObserved(input.Node);
            }

            OutputNode.AssignController(_circuitController, NodeClass.Simple);
            OutputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(OutputNode.Node);

            if (!_gateNodeId.HasValue)
                _gateNodeId = _circuitController.AddNode(GateNodeClass);

            foreach (var input in InputNodes)
                _circuitController.ConnectNodes(_gateNodeId.Value, input.NodeId);

            _circuitController.ConnectNodes(OutputNode.NodeId, _gateNodeId.Value);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (_circuitController == null) return;

            foreach (var input in InputNodes)
                foreach (var socket in input.GetComponentsInChildren<ConnectionSocket>())
                    socket.DisconnectAll();

            foreach (var socket in OutputNode.GetComponentsInChildren<ConnectionSocket>())
                socket.DisconnectAll();

            foreach (var input in InputNodes)
                _circuitController.DisconnectNodes(_gateNodeId.Value, input.NodeId);

            _circuitController.DisconnectNodes(OutputNode.NodeId, _gateNodeId.Value);

            _circuitController = null;
        }
    }
}
