using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{

    public abstract class Node
    {
        private List<Node> _inputs = new();
        private List<Node> _outputs = new();

        public bool TryAddInput(Node input)
        {
            if (!CanAddAsInput(input))
                return false;
            
            _inputs.Add(input);
            return true;
        }

        public bool TryAddOutput(Node output)
        {
            if (!CanAddAsOutput(output))
                return false;
            
            _outputs.Add(output);
            return true;
        }

        public virtual bool CanAddAsInput(Node input)
        {
            return true;
        }

        public virtual bool CanAddAsOutput(Node output)
        {
            return true;
        }

        public abstract bool? Evaluate();
    }

    public class InputNode : Node
    {
        public bool? value;
        
        public override bool CanAddAsInput(Node input)
        {
            return false;
        }

        public override bool? Evaluate()
        {
            return value;
        }
    }

    public class OutputNode : Node
    {
        public override bool CanAddAsOutput(Node output)
        {
            return false;
        }

        public override bool? Evaluate()
        {
            return true;
        }
    }

    public class Gate
    {
        
    }

    public class Circuit
    {
        public List<Node> nodes = new();

        public void AddNode(Node node)
        {
            
        }

        public void ConnectNodes(Node inputNode, Node outputNode)
        {
            
        }

        public void AddGate()
        {
            
        }
    }

public class CircuitController : MonoBehaviour
    {
        public void AddPort(Port newPort)
        {
                
        }

        public void AddGate()
        {
            
        }

        public void RemoveGate()
        {
            
        }

        public void StartWire(Port port, SelectEnterEventArgs args)
        {
            
        }

        public void ConnectPorts()
        {
            
        }    
    }
}