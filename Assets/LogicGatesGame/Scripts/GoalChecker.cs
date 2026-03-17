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

    public class GoalChecker : MonoBehaviour
    {
        [SerializeField] private string             expression;
        [SerializeField] private NodeComponent      sinkNode;
        [SerializeField] private List<VariableEntry> variableBindings;

        public event Action OnGoalAchieved;
        public event Action OnGoalLost;
        public bool IsGoalAchieved { get; private set; }

        private ExpressionEvaluator _evaluator;
        private SourceNode[]         _orderedSources;
        private CircuitController    _circuit;
        private bool[]               _evalBuffer;

        private void Start()
        {
            _evaluator = ExpressionEvaluator.Parse(expression);

            _orderedSources = new SourceNode[_evaluator.Variables.Count];
            for (int i = 0; i < _evaluator.Variables.Count; i++)
            {
                string varName = _evaluator.Variables[i];
                VariableEntry entry = variableBindings.Find(e => e.variableName == varName);
                if (entry == null || entry.sourceNode == null)
                {
                    Debug.LogError($"[GoalChecker] No binding for variable '{varName}'.");
                    return;
                }
                if (entry.sourceNode.Node is not SourceNode sn)
                {
                    Debug.LogError($"[GoalChecker] Node bound to '{varName}' is not a SourceNode.");
                    return;
                }
                _orderedSources[i] = sn;
            }

            _evalBuffer = new bool[_evaluator.Variables.Count];
            _circuit = GetComponent<CircuitController>();
            if (_circuit == null)
            {
                Debug.LogError("[GoalChecker] No CircuitController found on this GameObject.");
                return;
            }

            _circuit.OnCircuitChanged += OnCircuitChanged;
        }

        private void OnDestroy()
        {
            if (_circuit != null)
                _circuit.OnCircuitChanged -= OnCircuitChanged;
        }

        private void OnCircuitChanged()
        {
            bool achieved = CheckGoal();
            if (achieved && !IsGoalAchieved)
            {
                IsGoalAchieved = true;
                OnGoalAchieved?.Invoke();
            }
            else if (!achieved && IsGoalAchieved)
            {
                IsGoalAchieved = false;
                OnGoalLost?.Invoke();
            }
        }

        private bool CheckGoal()
        {
            if (_evaluator == null || _orderedSources == null)
                return false;

            if (sinkNode == null || sinkNode.Node is not SinkNode sink)
                return false;

            if (sink.Inputs.Count == 0)
                return false;

            int n = _orderedSources.Length;

            // Snapshot current source values
            bool?[] buffer = new bool?[n];
            for (int i = 0; i < n; i++)
                buffer[i] = _orderedSources[i].value;

            // Walk all 2^n combinations
            for (int combo = 0; combo < (1 << n); combo++)
            {
                for (int i = 0; i < n; i++)
                {
                    bool v = (combo >> i & 1) == 1;
                    _orderedSources[i].setValue(v);
                    _evalBuffer[i] = v;
                }

                bool? circuitResult = sink.ExecEvaluation();
                bool  exprResult    = _evaluator.Evaluate(_evalBuffer);

                if (circuitResult == null || circuitResult.Value != exprResult)
                {
                    RestoreBuffer(buffer);
                    return false;
                }
            }

            RestoreBuffer(buffer);
            return true;
        }

        private void RestoreBuffer(bool?[] buffer)
        {
            for (int i = 0; i < _orderedSources.Length; i++)
                _orderedSources[i].value = buffer[i];
        }
    }
}
