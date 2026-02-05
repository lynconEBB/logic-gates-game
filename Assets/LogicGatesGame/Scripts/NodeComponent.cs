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
        
        [SerializeField]
        private SourceProvider sourceProvider;
        

        private CircuitController _circuitController;
        
        private void Awake()
        {
            _circuitController = GetComponentInParent<CircuitController>();
            _nodeId = _circuitController.AddNode(type);
            if (type == NodeType.Output && sourceProvider != null)
            {
                sourceProvider.OnValueChanged += arg0 =>
                {
                    _circuitController.UpdateValue(_nodeId, arg0);
                };
            }
        }
        
        public bool CanConnect(int? otherNode)
        {
            if (!otherNode.HasValue)
                return true;
            
            int inputNode = type == NodeType.Input ? NodeId : otherNode.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNode.Value;
            
            return _circuitController.CanConnectNodes(inputNode, outputNode);
        }

        public void ConnectTo(int? otherNodeId)
        {
            if (!otherNodeId.HasValue)
                return;
                
            int inputNode = type == NodeType.Input ? NodeId : otherNodeId.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNodeId.Value;
            
            _circuitController.ConnectNodes(inputNode, outputNode);
        }

        public bool DisconnectFrom(int? otherNode)
        {
            if (!otherNode.HasValue)
                return true;
            
            int inputNode = type == NodeType.Input ? NodeId : otherNode.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNode.Value;
            
            return _circuitController.DisconnectNodes(inputNode, outputNode);
            
        }
    }
}