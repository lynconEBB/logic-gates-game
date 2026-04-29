using TMPro;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameManager gameManager;

        public void Show(float score)
        {
            if (timeText != null)
            {
                int totalSeconds = gameManager != null ? gameManager.ElapsedSeconds : 0;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            if (scoreText != null)
                scoreText.text = Mathf.RoundToInt(score).ToString();

            gameObject.SetActive(true);
        }
    }
}
