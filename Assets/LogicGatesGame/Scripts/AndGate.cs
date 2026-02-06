using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class AndGate : XRGrabInteractable
    {
        [SerializeField]
        private NodeComponent inputNodeA;
        [SerializeField]
        private NodeComponent inputNodeB;
        [SerializeField]
        private NodeComponent outputNode;
        
        private Node _notNode;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is GateSocket socket)
            {
                /*
                inputNode.AssignController(socket.CircuitController, NodeClass.Simple);
                inputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(inputNode.Node);
                outputNode.AssignController(socket.CircuitController, NodeClass.Simple);
                outputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(outputNode.Node);

                
                int notNodeId = socket.CircuitController.AddNode(NodeClass.Not);
                socket.CircuitController.ConnectNodes(notNodeId, inputNode.NodeId);
                socket.CircuitController.ConnectNodes(outputNode.NodeId, notNodeId);
            */
            }
        }
    }
}