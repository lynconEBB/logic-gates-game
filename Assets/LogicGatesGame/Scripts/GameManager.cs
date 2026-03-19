using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LogicGatesGame.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        public event Action<int> OnSecondTick;

        public float ElapsedTime { get; private set; }
        public int ElapsedSeconds { get; private set; }

        private bool _timerRunning;
        private int _lastSecond;

        public void StartTimer()
        {
            ElapsedTime = 0f;
            ElapsedSeconds = 0;
            _lastSecond = 0;
            _timerRunning = true;
        }

        public void StopTimer() => _timerRunning = false;

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

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
