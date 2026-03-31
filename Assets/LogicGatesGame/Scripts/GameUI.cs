using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private InProgressPanel inProgressPanel;
        [SerializeField] private ResultsPanel resultsPanel;
        [SerializeField] private GoalChecker goalChecker;

        private void OnEnable()
        {
            if (goalChecker != null)
                goalChecker.OnGoalAchieved += OnGoalAchieved;
        }

        private void OnDisable()
        {
            if (goalChecker != null)
                goalChecker.OnGoalAchieved -= OnGoalAchieved;
        }

        private void Start()
        {
            SetResultsVisible(goalChecker != null && goalChecker.IsGoalAchieved);
        }

        private void OnGoalAchieved() => SetResultsVisible(true);

        private void SetResultsVisible(bool show)
        {
            if (inProgressPanel != null)
                inProgressPanel.gameObject.SetActive(!show);
            if (resultsPanel != null)
                resultsPanel.gameObject.SetActive(show);
        }
    }
}
