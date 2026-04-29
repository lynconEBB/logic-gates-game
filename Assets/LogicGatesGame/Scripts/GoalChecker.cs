using System.Collections.Generic;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GoalChecker : MonoBehaviour
    {
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private GameManager gameManager;

        public bool IsGoalAchieved { get; private set; }

        private string             _expression;
        private NodeComponent      _sinkNode;
        private List<VariableEntry> _variableBindings;

        private ExpressionEvaluator _evaluator;
        private SourceNode[]         _orderedSources;
        private CircuitController    _circuit;
        private bool[]               _evalBuffer;

        private void Awake()
        {
            if (gameDirector != null)
            {
                gameDirector.OnCircuitReady += OnCircuitReady;
                if (gameDirector.SelectedCircuit != null)
                    OnCircuitReady(gameDirector.SelectedCircuit);
            }
        }

        private void OnCircuitReady(CircuitDefinition def)
        {
            _expression       = def.expression;
            _sinkNode         = def.sinkNode;
            _variableBindings = def.variableBindings;
            Initialize();
        }

        private void Initialize()
        {
            _evaluator = ExpressionEvaluator.Parse(_expression);

            _orderedSources = new SourceNode[_evaluator.Variables.Count];
            for (int i = 0; i < _evaluator.Variables.Count; i++)
            {
                string varName = _evaluator.Variables[i];
                VariableEntry entry = _variableBindings.Find(e => e.variableName == varName);
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
            _circuit = _sinkNode.GetComponentInParent<CircuitController>();
            if (_circuit == null)
            {
                Debug.LogError("[GoalChecker] No CircuitController found on the circuit.");
                return;
            }

            _circuit.OnCircuitChanged += OnCircuitChanged;
        }

        private void OnDestroy()
        {
            if (gameDirector != null)
                gameDirector.OnCircuitReady -= OnCircuitReady;
            if (_circuit != null)
                _circuit.OnCircuitChanged -= OnCircuitChanged;
        }

        private void OnCircuitChanged()
        {
            if (IsGoalAchieved)
                return;

            if (CheckGoal())
            {
                IsGoalAchieved = true;
                if (gameManager != null)
                    gameManager.NotifyGoalAchieved();
            }
        }

        private bool CheckGoal()
        {
            if (_evaluator == null || _orderedSources == null)
                return false;

            if (_sinkNode == null || _sinkNode.Node is not SinkNode sink)
                return false;

            if (sink.Inputs.Count == 0)
                return false;

            int n = _orderedSources.Length;

            bool?[] buffer = new bool?[n];
            for (int i = 0; i < n; i++)
                buffer[i] = _orderedSources[i].value;

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
