using System;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameManager : SceneSingleton<GameManager>
    {
        [SerializeField] private GoalChecker goalChecker;

        public event Action<int> OnSecondTick;

        public float ElapsedTime { get; private set; }
        public int ElapsedSeconds { get; private set; }

        private int _lastSecond;
        private bool _timerRunning;

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
            ElapsedTime = 0f;
            ElapsedSeconds = 0;
            _lastSecond = 0;
            _timerRunning = true;
        }

        private void Update()
        {
            if (!_timerRunning) return;

            ElapsedTime += Time.deltaTime;
            int seconds = Mathf.FloorToInt(ElapsedTime);
            if (seconds != _lastSecond)
            {
                _lastSecond = seconds;
                ElapsedSeconds = seconds;
                OnSecondTick?.Invoke(seconds);
            }
        }

        private void OnGoalAchieved() => _timerRunning = false;
    }
}
