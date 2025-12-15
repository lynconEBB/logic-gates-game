using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class Circuit
    {
        public int lastId = 0;
        private Dictionary<int, Node> _nodes = new();

        public int AddNode(NodeType type)
        {
            lastId++;
            Node node;
            switch (type)
            {
                case NodeType.Input:
                    node = new SinkNode(lastId);
                    break;
                case NodeType.Output:
                    node = new SourceNode(lastId);
                    break;
                default:
                    node = new SourceNode(lastId);
                    break;
            }
            _nodes.Add(lastId, node);
            return lastId;
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
            
            PrintState();
            return true;
        }
        
        public bool DisconnectNodes(int inputNode, int outputNode)
        {
            if (!AreNodesConnected(inputNode, outputNode))
                return false;
            
            _nodes[inputNode].Inputs.Remove(_nodes[outputNode]);
            _nodes[outputNode].Outputs.Remove(_nodes[inputNode]);
            
            PrintState();
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

    public class CircuitController : MonoBehaviour
    {
        private Circuit _circuit = new();

        public int AddNode(NodeType type)
        {
            return _circuit.AddNode(type);
        }

        public bool CanConnectNodes(int inputNodeId, int outputNodeId)
        {
            return _circuit.CanConnectNodes(inputNodeId, outputNodeId); 
        }

        public bool ConnectNodes(int inputNodeId, int outputNodeId)
        {
            return _circuit.ConnectNodes(inputNodeId, outputNodeId);    
        }

        public void AddGate()
        {
            
        }

        public void RemoveGate()
        {
            
        }

        public bool DisconnectNodes(int inputNode, int outputNode)
        {
            return _circuit.DisconnectNodes(inputNode, outputNode);
        }
    }
}