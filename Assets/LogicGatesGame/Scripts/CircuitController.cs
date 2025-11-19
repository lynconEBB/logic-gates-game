using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class Circuit
    {
        public int lastId = 0;
        public Dictionary<int, Node> nodes = new();

        public int AddNode(Node node)
        {
            lastId++;
            nodes.Add(lastId, node);
            return lastId;
        }

        public void RemoveNode(int nodeId)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
                return;

            foreach (Node input in node.Inputs)
            {
                input.Outputs.Remove(node);
            }
            foreach (Node output in node.Outputs)
            {
                output.Inputs.Remove(node);
            }
            nodes.Remove(nodeId);
        }

        public bool ConnectNodes(int inputNodeId, int outputNodeId)
        {
            if (!nodes.ContainsKey(inputNodeId) || !nodes.ContainsKey(outputNodeId) || 
                !nodes[inputNodeId].CanAddAsOutput(nodes[outputNodeId]) || !nodes[outputNodeId].CanAddAsInput(nodes[inputNodeId]))
                return false;
            
            nodes[inputNodeId].TryAddOutput(nodes[outputNodeId]);
            nodes[outputNodeId].TryAddInput(nodes[inputNodeId]);
            return true;
        }
    }

    public class CircuitController : MonoBehaviour
    {
        private Circuit _circuit = new();
        
        public int AddNode(Node newNode)
        {
            return _circuit.AddNode(newNode);         
        }

        public void AddGate()
        {
            
        }

        public void RemoveGate()
        {
            
        }

        public void ConnectPorts()
        {
            
        }    
    }
}