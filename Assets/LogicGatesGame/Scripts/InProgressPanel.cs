using TMPro;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class InProgressPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text expressionText;
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private GameManager gameManager;
        

        private void Awake()
        {
            if (gameDirector != null)
            {
                gameDirector.OnCircuitReady += OnCircuitReady;
                if (gameDirector.SelectedCircuit != null)
                    OnCircuitReady(gameDirector.SelectedCircuit);
            }
        }

        private void OnDestroy()
        {
            if (gameDirector != null)
                gameDirector.OnCircuitReady -= OnCircuitReady;
        }

        private void OnCircuitReady(CircuitDefinition def)
        {
            if (expressionText != null)
                expressionText.text = def.expression;
        }

        private bool _started;

        private void Start()
        {
            _started = true;
            Subscribe();
        }

        private void OnEnable()
        {
            if (_started) Subscribe();
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.OnSecondTick -= UpdateTimeDisplay;
        }

        private void Subscribe()
        {
            if (gameManager == null) return;
            gameManager.OnSecondTick += UpdateTimeDisplay;
            UpdateTimeDisplay(gameManager.ElapsedSeconds);
        }

        private void UpdateTimeDisplay(int totalSeconds)
        {
            if (timeText == null) return;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
