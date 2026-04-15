using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class GameManager : SceneSingleton<GameManager>
    {
        [SerializeField] private GoalChecker goalChecker;
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private XRInteractionManager interactionManager;

        public event Action<int> OnSecondTick;
        public event Action OnGameFinished;

        public float ElapsedTime { get; private set; }
        public int ElapsedSeconds { get; private set; }

        private int _lastSecond;
        private bool _timerRunning;
        private bool _telemetrySaved;

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
            _telemetrySaved = false;
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

        private void OnGoalAchieved()
        {
            _timerRunning = false;
            DisablePlayerInteraction();
            SaveTelemetrySnapshot();
            OnGameFinished?.Invoke();
        }

        private void SaveTelemetrySnapshot()
        {
            if (_telemetrySaved)
                return;

            var telemetry = TelemetryManager.Instance;
            if (telemetry == null)
            {
                Debug.LogWarning("[GameManager] TelemetryManager instance was not available when saving telemetry.");
                return;
            }

            var record = TelemetrySessionRecord.Create(
                ElapsedSeconds,
                SceneManager.GetActiveScene().name,
                gameDirector != null ? gameDirector.SelectedExpression : string.Empty,
                telemetry.GetAll());

            TelemetryLocalStore.SaveCompletedSession(record);
            _telemetrySaved = true;
            TelemetryFirestoreSync.Instance?.TrySyncPendingSessions();
        }

        private void DisablePlayerInteraction()
        {
            if (interactionManager == null) return;

            var interactors = new List<IXRInteractor>();
            interactionManager.GetRegisteredInteractors(interactors);

            int defaultMask = InteractionLayerMask.GetMask("Default");
            foreach (var interactor in interactors)
            {
                if (interactor is not (NearFarInteractor or XRPokeInteractor)) continue;
                if (interactor is not XRBaseInteractor baseInteractor) continue;

                baseInteractor.interactionLayers = new InteractionLayerMask
                {
                    value = baseInteractor.interactionLayers.value & ~defaultMask
                };
            }
        }
    }
}
