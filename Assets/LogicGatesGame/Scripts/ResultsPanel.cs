using TMPro;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;

        private void OnEnable()
        {
            if (timeText == null) return;
            int totalSeconds = GameManager.Instance != null ? GameManager.Instance.ElapsedSeconds : 0;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
