using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private InProgressPanel inProgressPanel;
        [SerializeField] private ResultsPanel resultsPanel;
        [SerializeField] private GameManager gameManager;

        private void OnEnable()
        {
            if (gameManager != null)
                gameManager.ResultReady += OnResultReady;
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.ResultReady -= OnResultReady;
        }

        private void Start()
        {
            if (gameManager != null && gameManager.HasResult)
                ShowResults(gameManager.LastScore);
            else
                ShowResults(null);
        }

        private void OnResultReady(float score) => ShowResults(score);

        private void ShowResults(float? score)
        {
            bool show = score.HasValue;

            if (inProgressPanel != null)
                inProgressPanel.gameObject.SetActive(!show);

            if (resultsPanel == null)
                return;

            if (show)
                resultsPanel.Show(score.Value);
            else
                resultsPanel.gameObject.SetActive(false);
        }
    }
}
