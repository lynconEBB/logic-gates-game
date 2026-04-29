using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private XRInteractionManager interactionManager;

        public event Action<int> OnSecondTick;
        public event Action OnGameFinished;
        public event Action<float> ResultReady;

        public float ElapsedTime { get; private set; }
        public int ElapsedSeconds { get; private set; }
        public bool IsGameFinished { get; private set; }
        public bool HasResult { get; private set; }
        public float LastScore { get; private set; }

        private int _lastSecond;
        private bool _timerRunning;

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

        public void NotifyGoalAchieved()
        {
            if (IsGameFinished)
                return;

            IsGameFinished = true;
            _timerRunning = false;
            DisablePlayerInteraction();
            OnGameFinished?.Invoke();
        }

        public void NotifyResultReady(float score)
        {
            if (HasResult)
                return;

            HasResult = true;
            LastScore = score;
            ResultReady?.Invoke(score);
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
