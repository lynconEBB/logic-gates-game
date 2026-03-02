using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;

namespace LogicGatesGame.Scripts
{

    public enum NodeClass
    {
        Source,
        Sink,
        Simple,
        And,
        Or,
        Not
    }
    
    public abstract class Node
    {
        private readonly int _id;
        public int Id => _id;
        
        public virtual int? maxInputs => null;
        public virtual int? maxOutputs => null;
        
        protected List<Node> inputs = new();
        protected List<Node> outputs = new();
        
        public List<Node> Inputs => inputs;
        public List<Node> Outputs => outputs;
        
        public event Action<bool?> OnEvaluated;

        public Node(int id)
        {
            _id = id;
        }

        public bool TryAddInput(Node input)
        {
            if (!CanAddToInputSlot(input))
                return false;
            
            inputs.Add(input);
            return true;
        }

        public bool TryAddOutput(Node output)
        {
            if (!CanAddToOutputSlot(output))
                return false;
            
            outputs.Add(output);
            return true;
        }

        public virtual bool CanAddToOutputSlot(Node node)
        {
            return maxOutputs == null || outputs.Count < maxOutputs;
        }
        
        public virtual bool CanAddToInputSlot(Node node)
        {
            return maxInputs == null || inputs.Count < maxInputs;
        }

        public bool? Evaluate()
        {
            bool? result = ExecEvaluation();
            OnEvaluated?.Invoke(result);
            return result;
        }
        
        public abstract bool? ExecEvaluation();
    }

    public class SourceNode : Node
    {
        public bool? value = null;

        public SourceNode(int id) : base(id)
        {
        }

        public void setValue(bool newVal)
        {
            value = newVal; 
        }

        public override int? maxInputs => 0;

        public override bool? ExecEvaluation()
        {
            return value;
        }
    }

    public class SinkNode : Node
    {
        public SinkNode(int id) : base(id)
        { }

        public override int? maxOutputs => 0;

        public override int? maxInputs => 1;
        
        public override bool? ExecEvaluation()
        {
            if (inputs.Count == 0)
                return null;
            return inputs[0].ExecEvaluation();
        }
    }
    public class SimpleNode : Node
    {
        public SimpleNode(int id) : base(id) {}

        public override int? maxInputs => 1;

        public override bool? ExecEvaluation()
        {
            if (inputs.Count == 0)
                return null;
            return inputs[0].ExecEvaluation();
        }
    }

    public class AndNode : Node
    {
        public AndNode(int id) : base(id)
        {
        }

        public override bool? ExecEvaluation()
        {
            if (inputs.Count == 0)
                return null;
            
            foreach (Node node in inputs)
            {
                bool? nodeVal = node.ExecEvaluation();
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
        public OrNode(int id) : base(id)
        {
        }

        public override bool? ExecEvaluation()
        {
            if (inputs.Count == 0)
                return null;
            foreach (Node node in inputs)
            {
                bool? nodeVal = node.ExecEvaluation();
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
        public NotNode(int id) : base(id)
        {
        }

        public override int? maxInputs => 1;

        public override bool? ExecEvaluation()
        {
            if (inputs.Count == 0)
                return null;
            bool? inVal = inputs[0].ExecEvaluation();
            if (!inVal.HasValue)
                return null;
            return !inVal;
        }
    }
}