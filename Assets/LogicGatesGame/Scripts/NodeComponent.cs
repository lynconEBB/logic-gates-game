using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class NodeComponent : MonoBehaviour
    {
        [SerializeField]
        private NodeType type;
        public NodeType Type => type;
        
        private Node _node;
        public Node Node => _node;
        
        private int _nodeId;
        public int NodeId => _nodeId;

        private CircuitController _circuitController;
        
        private void Awake()
        {
            _circuitController = GetComponentInParent<CircuitController>();
            
            switch (type)
            {
                case NodeType.Input:
                    _node = new SinkNode();
                    break;
                case NodeType.Output:
                    _node = new SourceNode();
                    break;
            }
            
            _nodeId = _circuitController.AddNode(_node);
        }

        public void ConnectTo(int? otherNodeId)
        {
            if (!otherNodeId.HasValue)
                return;
                
            _circuitController.ConnectNodes(_nodeId, otherNodeId.Value);
        }

        public bool CanConnect(int? otherNode)
        {
            if (!otherNode.HasValue)
                return true;
            
            int inputNode = type == NodeType.Input ? NodeId : otherNode.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNode.Value;
            
            return _circuitController.CanConnectNodes(inputNode, outputNode);
        }
    }
}