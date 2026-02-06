using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class CircuitController : MonoBehaviour
    {
        private int _lastId = 0;
        private Dictionary<int, Node> _nodes = new();

        public Node GetNode(int nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) 
                return null;
            
            return node;
        }
        
        public void UpdateValue(int nodeId, bool newVal)
        {
            if (_nodes.TryGetValue(nodeId, out Node node) && node is SourceNode sourceNode)
            {
                sourceNode.setValue(newVal);        
                EvaluateTree(sourceNode);
            }
        }

        private void EvaluateTree(Node rootNode)
        {
            if (rootNode == null)
                return;
            
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            
            visited.Add(rootNode);
            queue.Enqueue(rootNode);
            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();
                current.Evaluate();
                
                foreach (Node outNode in current.Outputs)
                {
                    if (!visited.Contains(outNode))
                    {
                        visited.Add(outNode);
                        queue.Enqueue(outNode);
                    }
                }
            }
        }
        
        public int AddNode(NodeClass nodeClass)
        {
            _lastId++;
            Node node;
            switch (nodeClass)
            {
                case NodeClass.Sink:
                    node = new SinkNode(_lastId);
                    break;
                case NodeClass.Source:
                    node = new SourceNode(_lastId);
                    break;
                case NodeClass.Simple:
                    node = new SimpleNode(_lastId);
                    break;
                case NodeClass.And:
                    node = new AndNode(_lastId);
                    break;
                case NodeClass.Or:
                    node = new OrNode(_lastId);
                    break;
                case NodeClass.Not:
                    node = new NotNode(_lastId);
                    break;
                default:
                    node = new SourceNode(_lastId);
                    break;
            }
            _nodes.Add(_lastId, node);
            return _lastId;
        }

        public void RemoveNode(int nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node))
                return;

            foreach (Node input in node.Inputs)
            {
                input.Outputs.Remove(node);
            }

            foreach (Node output in node.Outputs)
            {
                output.Inputs.Remove(node);
            }

            _nodes.Remove(nodeId);
        }

        public bool ConnectNodes(int inputNodeId, int outputNodeId)
        {
            if (!CanConnectNodes(inputNodeId, outputNodeId))
                return false;

            _nodes[inputNodeId].TryAddInput(_nodes[outputNodeId]);
            _nodes[outputNodeId].TryAddOutput(_nodes[inputNodeId]);
            EvaluateTree(_nodes[inputNodeId]);
            
            return true;
        }
        
        public bool DisconnectNodes(int inputNode, int outputNode)
        {
            if (!AreNodesConnected(inputNode, outputNode))
                return false;
            
            _nodes[inputNode].Inputs.Remove(_nodes[outputNode]);
            _nodes[outputNode].Outputs.Remove(_nodes[inputNode]);
            EvaluateTree(_nodes[inputNode]);
            
            return true;
        }

        private bool AreNodesConnected(int inputNodeId, int outputNodeId)
        {
            return _nodes[inputNodeId].Inputs.Contains(_nodes[outputNodeId]) && 
                   _nodes[outputNodeId].Outputs.Contains(_nodes[inputNodeId]); 
        }

        public bool CanConnectNodes(int inputNodeId, int outputNodeId)
        {
            return _nodes.ContainsKey(inputNodeId) && _nodes.ContainsKey(outputNodeId) &&
                   _nodes[inputNodeId].CanAddToInputSlot(_nodes[outputNodeId]) &&
                   _nodes[outputNodeId].CanAddToOutputSlot(_nodes[inputNodeId]);
        }

        public void PrintState()
        {
            foreach (var node in _nodes)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Node {node.Key}");
                if (node.Value.Inputs.Count > 0)
                {
                    sb.Append($"Inputs: ");
                    foreach (var input in node.Value.Inputs)
                    {
                        sb.Append(input.Id);
                        sb.Append(", ");
                    }
                    sb.Append("\n");
                }
                else
                {
                    sb.AppendLine("No inputs");
                }
                
                if (node.Value.Outputs.Count > 0)
                {
                    sb.Append($"Ouputs: ");
                    foreach (var output in node.Value.Outputs)
                    {
                        sb.Append(output.Id);
                        sb.Append(", ");
                    }
                    sb.Append("\n");
                }
                else
                {
                    sb.AppendLine("No outputs");
                }
                Debug.Log(sb.ToString());
            } 
        }
    }
}