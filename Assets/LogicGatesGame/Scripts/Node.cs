using System.Collections.Generic;

namespace LogicGatesGame.Scripts
{
    public enum NodeType
    {
        SOURCE,
        SINK,
        SIMPLE,
        AND,
        OR,
        NOT
    }
    
    public abstract class Node
    {
        public virtual int? maxInputs => null;
        public virtual int? maxOutputs => null;
        
        
        protected List<Node> inputs = new();
        protected List<Node> outputs = new();
        
        public List<Node> Inputs => inputs;
        public List<Node> Outputs => outputs;

        public bool TryAddInput(Node input)
        {
            if (maxInputs != null && inputs.Count >= maxInputs)
                return false;
            
            inputs.Add(input);
            return true;
        }

        public bool TryAddOutput(Node output)
        {
            if (maxOutputs != null && outputs.Count >= maxOutputs)
                return false;
            
            outputs.Add(output);
            return true;
        }

        public virtual bool CanAddAsOutput(Node node)
        {
            return maxOutputs == null || outputs.Count < maxOutputs;
        }
        
        public virtual bool CanAddAsInput(Node node)
        {
            return maxInputs == null || inputs.Count < maxInputs;
        }

        public abstract bool? Evaluate();
    }

    public class SourceNode : Node
    {
        public bool value;

        public override int? maxInputs => 0;

        public override bool? Evaluate()
        {
            return value;
        }
    }

    public class SinkNode : Node
    {
        public override int? maxOutputs => 0;

        public override int? maxInputs => 1;
        
        public override bool? Evaluate()
        {
            return true;
        }
    }

    public class SimpleNode : Node
    {
        public override int? maxInputs => 1;

        public override bool? Evaluate()
        {
            if (inputs.Count == 0)
                return null;
            bool? inVal = inputs[0].Evaluate();
            if (!inVal.HasValue)
                return null;
            return inVal;
        }
    }

    public class AndNode : Node
    {
        public override bool? Evaluate()
        {
            if (inputs.Count == 0)
                return null;
            
            foreach (Node node in inputs)
            {
                bool? nodeVal = node.Evaluate();
                if (!nodeVal.HasValue)
                    return null;
                if (nodeVal.Value == false)
                {
                    return false;
                }
            }     
            return true;
        }
    }

    public class OrNode : Node
    {
        public override bool? Evaluate()
        {
            if (inputs.Count == 0)
                return null;
            foreach (Node node in inputs)
            {
                bool? nodeVal = node.Evaluate();
                if (!nodeVal.HasValue)
                    return null;
                if (nodeVal.Value == true)
                {
                    return true;
                }
            }     
            return false;
        }
    }
    
    public class NotNode : Node
    {
        public override int? maxInputs => 1;

        public override bool? Evaluate()
        {
            if (inputs.Count == 0)
                return null;
            bool? inVal = inputs[0].Evaluate();
            if (!inVal.HasValue)
                return null;
            return !inVal;
        }
    }
}