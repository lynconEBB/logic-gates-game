using System;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class NodeComponent : MonoBehaviour
    {
        [SerializeField]
        private NodeType type;
        private CircuitController _circuitController;
        private int _nodeId;
        private Node _node;

        private void Awake()
        {
            _circuitController = GetComponentInParent<CircuitController>();
            
            switch (type)
            {
                case NodeType.SOURCE:
                    _node = new SourceNode();
                    break;
                case NodeType.SINK:
                    _node = new SinkNode();
                    break;
                case NodeType.AND:
                    _node = new AndNode();
                    break;
                case NodeType.OR:
                    _node = new OrNode();
                    break;
                case NodeType.NOT:
                    _node = new NotNode();
                    break;
                case NodeType.SIMPLE:
                default:
                    _node = new SimpleNode();
                    break;
            }
            
            _nodeId = _circuitController.AddNode(_node);
        }
        

        public bool IsAcceptingOutput()
        {
            return true;
        }
    }
}