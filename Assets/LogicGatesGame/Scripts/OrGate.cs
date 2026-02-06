using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class OrGate : XRGrabInteractable
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
                inputNodeA.AssignController(socket.CircuitController, NodeClass.Simple);
                inputNodeA.GetComponentInChildren<StateVisualizer>().SetNodeObserved(inputNodeA.Node);
                
                inputNodeB.AssignController(socket.CircuitController, NodeClass.Simple);
                inputNodeB.GetComponentInChildren<StateVisualizer>().SetNodeObserved(inputNodeB.Node);
                
                outputNode.AssignController(socket.CircuitController, NodeClass.Simple);
                outputNode.GetComponentInChildren<StateVisualizer>().SetNodeObserved(outputNode.Node);
                
                int andNodeId = socket.CircuitController.AddNode(NodeClass.Or);
                socket.CircuitController.ConnectNodes(andNodeId, inputNodeA.NodeId);
                socket.CircuitController.ConnectNodes(andNodeId, inputNodeB.NodeId);
                socket.CircuitController.ConnectNodes(outputNode.NodeId, andNodeId);
            }
        }
    }
}