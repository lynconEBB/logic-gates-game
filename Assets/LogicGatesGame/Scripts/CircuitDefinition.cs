using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [Serializable]
    public class VariableEntry
    {
        public string        variableName;
        public NodeComponent sourceNode;
    }

    public class CircuitDefinition : MonoBehaviour
    {
        [SerializeField] public string               expression;
        [SerializeField] public NodeComponent        sinkNode;
        [SerializeField] public List<VariableEntry>  variableBindings;

        [SerializeField] public int                  idealGates;
        [SerializeField] public int                  idealConnections;
        [SerializeField] public int                  idealTime;
    }
}
