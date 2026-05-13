using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LogicGatesGame.Scripts
{
    public class GameDirector : MonoBehaviour
    {
        [SerializeField] private List<CircuitDefinition> easyCircuits;
        [SerializeField] private List<CircuitDefinition> mediumCircuits;
        [SerializeField] private List<CircuitDefinition> hardCircuits;

        public event Action<CircuitDefinition> OnCircuitReady;
        public CircuitDefinition SelectedCircuit { get; private set; }
        public string SelectedExpression => SelectedCircuit?.expression;

        private void Awake()
        {
            List<CircuitDefinition> list = GetCircuitsForDifficulty(DifficultyManager.SelectedDifficulty);

            if (list == null || list.Count == 0)
            {
                Debug.LogError($"[GameDirector] No {DifficultyManager.SelectedDifficulty} circuits configured.");
                return;
            }

            CircuitDefinition prefab = list[Random.Range(0, list.Count)];
            SelectedCircuit = Instantiate(prefab.gameObject).GetComponent<CircuitDefinition>();
            OnCircuitReady?.Invoke(SelectedCircuit);
        }

        private List<CircuitDefinition> GetCircuitsForDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy: return easyCircuits;
                case Difficulty.Medium: return mediumCircuits;
                case Difficulty.Hard: return hardCircuits;
                default: return null;
            }
        }
    }
}
