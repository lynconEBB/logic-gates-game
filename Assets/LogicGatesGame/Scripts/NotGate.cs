using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class NotGate : Gate
    {
        [SerializeField] private NodeComponent inputNode;
        [SerializeField] private NodeComponent outputNode;

        protected override NodeClass GateNodeClass => NodeClass.Not;
        protected override NodeComponent[] InputNodes => new[] { inputNode };
        protected override NodeComponent OutputNode => outputNode;
    }
}
