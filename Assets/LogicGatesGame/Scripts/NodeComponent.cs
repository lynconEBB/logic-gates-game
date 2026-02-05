using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public enum NodeType
    {
        Output,
        Input,
    }
    
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
            AssignController(GetComponentInParent<CircuitController>(), Type == NodeType.Input ? NodeClass.Sink : NodeClass.Source);
            
            if (type == NodeType.Output && sourceProvider != null)
            {
                sourceProvider.OnValueChanged += state =>
                {
                    _circuitController.UpdateValue(_nodeId, state);
                };
            }
        }

        public void AssignController(CircuitController controller, NodeClass nodeClass)
        {
            _circuitController = controller;
            if (!_circuitController) 
                return;
            
            _nodeId = _circuitController.AddNode(nodeClass);
            _node = _circuitController.GetNode(_nodeId);
        }
        
        public bool CanConnect(int? otherNode)
        {
            if (_node == null)
                return false;
            
            if (!otherNode.HasValue)
                return true;
            
            int inputNode = type == NodeType.Input ? NodeId : otherNode.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNode.Value;
            
            return _circuitController.CanConnectNodes(inputNode, outputNode);
        }

        public void ConnectTo(int? otherNodeId)
        {
            if (!otherNodeId.HasValue || _node == null)
                return;
                
            int inputNode = type == NodeType.Input ? NodeId : otherNodeId.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNodeId.Value;
            
            _circuitController.ConnectNodes(inputNode, outputNode);
        }

        public void DisconnectFrom(int? otherNode)
        {
            if (!otherNode.HasValue || _node == null)
                return;
            
            int inputNode = type == NodeType.Input ? NodeId : otherNode.Value;
            int outputNode = type == NodeType.Output ? NodeId : otherNode.Value;
            
            _circuitController.DisconnectNodes(inputNode, outputNode);
        }
    }
}