using TMPro;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private GoalChecker goalChecker;

        [SerializeField] private string goalAchievedMessage = "Expression found!";
        [SerializeField] private string goalLostMessage = "";

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnSecondTick += OnSecondTick;

            if (goalChecker != null)
            {
                goalChecker.OnGoalAchieved += OnGoalAchieved;
                goalChecker.OnGoalLost += OnGoalLost;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnSecondTick -= OnSecondTick;

            if (goalChecker != null)
            {
                goalChecker.OnGoalAchieved -= OnGoalAchieved;
                goalChecker.OnGoalLost -= OnGoalLost;
            }
        }

        private void Start()
        {
            UpdateTimeDisplay(GameManager.Instance != null ? GameManager.Instance.ElapsedSeconds : 0);
            UpdateGoalDisplay(goalChecker != null && goalChecker.IsGoalAchieved);
        }

        private void OnSecondTick(int seconds) => UpdateTimeDisplay(seconds);

        private void OnGoalAchieved() => UpdateGoalDisplay(true);

        private void OnGoalLost() => UpdateGoalDisplay(false);

        private void UpdateTimeDisplay(int totalSeconds)
        {
            if (timeText == null) return;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        private void UpdateGoalDisplay(bool achieved)
        {
            if (goalText == null) return;
            goalText.text = achieved ? goalAchievedMessage : goalLostMessage;
        }
    }
}
