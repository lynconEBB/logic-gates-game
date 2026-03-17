using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class AndGate : Gate
    {
        [SerializeField] private NodeComponent inputNodeA;
        [SerializeField] private NodeComponent inputNodeB;
        [SerializeField] private NodeComponent outputNode;

        protected override NodeClass GateNodeClass => NodeClass.And;
        protected override NodeComponent[] InputNodes => new[] { inputNodeA, inputNodeB };
        protected override NodeComponent OutputNode => outputNode;
    }
}
