using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class NotGate : XRGrabInteractable
    {
        [SerializeField]
        private NodeComponent inputNode;
        [SerializeField]
        private NodeComponent outputNode;
        
        private Node _notNode;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is GateSocket socket)
            {
                inputNode.AssignController(socket.CircuitController, NodeClass.Simple);
                inputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(inputNode.Node);
                outputNode.AssignController(socket.CircuitController, NodeClass.Source);
                outputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(outputNode.Node);

                /*
                int notNodeId = socket.CircuitController.AddNode(NodeType.Not);
                socket.CircuitController.ConnectNodes(inputNode.NodeId, notNodeId);
                socket.CircuitController.ConnectNodes(notNodeId, outputNode.NodeId);
            */
            }
        }
    }
}