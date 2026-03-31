using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LogicGatesGame.Scripts
{
    public class GameDirector : MonoBehaviour
    {
        [SerializeField] private List<CircuitDefinition> circuits;

        public event Action<CircuitDefinition> OnCircuitReady;
        public CircuitDefinition SelectedCircuit { get; private set; }
        public string SelectedExpression => SelectedCircuit?.expression;

        private void Awake()
        {
            if (circuits == null || circuits.Count == 0)
            {
                Debug.LogError("[GameDirector] No circuits configured.");
                return;
            }

            CircuitDefinition prefab = circuits[Random.Range(0, circuits.Count)];
            SelectedCircuit = Instantiate(prefab.gameObject).GetComponent<CircuitDefinition>();
            OnCircuitReady?.Invoke(SelectedCircuit);
        }
    }
}
