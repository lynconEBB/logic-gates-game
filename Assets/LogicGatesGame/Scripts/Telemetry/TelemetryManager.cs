using System.Collections.Generic;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class TelemetryManager : SceneSingleton<TelemetryManager>
    {
        public const string KeyGates = "gates";
        public const string KeyConnectionCanceled = "connectionCanceled";
        public const string KeyConnectionFailed = "connectionFailed";
        public const string KeyConnectionSuccessful = "connectionSuccessful";
        public const string KeyDisconnections = "disconnections";

        private const float ScoreBase = 1000f;
        private const float WeightTime = 0.02f;
        private const float WeightGates = 1.5f;
        private const float WeightConnections = 0.75f;
        private const float WeightFailures = 2.0f;
        private const float WeightCanceled = 0.75f;
        private const float WeightDisconnections = 1.0f;

        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameDirector gameDirector;

        private readonly Dictionary<string, int> _data = new Dictionary<string, int>();
        private bool _telemetrySaved;

        private void Start()
        {
            RegisterKey(KeyGates);
            RegisterKey(KeyDisconnections);
            RegisterKey(KeyConnectionCanceled);
            RegisterKey(KeyConnectionFailed);
            RegisterKey(KeyConnectionSuccessful);
        }

        private void OnEnable()
        {
            if (gameManager != null)
                gameManager.OnGameFinished += OnGameFinished;
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.OnGameFinished -= OnGameFinished;
        }

        private void RegisterKey(string key)
        {
            if (_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' is already registered.");
                return;
            }
            _data[key] = 0;
        }

        public void Increment(string key)
        {
            if (!_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' not found.");
                return;
            }
            _data[key]++;
        }

        public void Decrement(string key)
        {
            if (!_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' not found.");
                return;
            }
            _data[key] = Mathf.Max(0, _data[key] - 1);
        }

        public int GetCount(string key)
        {
            _data.TryGetValue(key, out var count);
            return count;
        }

        public IReadOnlyDictionary<string, int> GetAll() => _data;

        private void OnGameFinished()
        {
            if (_telemetrySaved)
                return;

            var record = TelemetrySessionRecord.Create(
                gameManager != null ? gameManager.ElapsedSeconds : 0,
                gameDirector != null ? gameDirector.SelectedExpression : string.Empty,
                GetCount(KeyGates),
                GetCount(KeyDisconnections),
                GetCount(KeyConnectionCanceled),
                GetCount(KeyConnectionFailed),
                GetCount(KeyConnectionSuccessful),
                CalculateScore());

            if (gameManager != null)
                gameManager.NotifyResultReady(record.score);
            
            TelemetryLocalStore.SaveCompletedSession(record);
            _telemetrySaved = true;
            TelemetryFirestoreSync.Instance?.TrySyncPendingSessions();
        }

        public float CalculateScore()
        {
            CircuitDefinition circuit = gameDirector != null ? gameDirector.SelectedCircuit : null;
            int idealGates = circuit != null ? circuit.idealGates : 0;
            int idealConnections = circuit != null ? circuit.idealConnections : 0;
            int idealTime = circuit != null ? circuit.idealTime : 0;

            int gatesUsed = GetCount(KeyGates);
            int connectionsMade = GetCount(KeyConnectionSuccessful);
            int connectionsFailed = GetCount(KeyConnectionFailed);
            int connectionsCanceled = GetCount(KeyConnectionCanceled);
            int disconnectionsCount = GetCount(KeyDisconnections);
            int completionTime = gameManager != null ? gameManager.ElapsedSeconds : 0;

            float gatesPenalty = Mathf.Max(0, gatesUsed - idealGates) * WeightGates;
            float connectionsPenalty = Mathf.Max(0, connectionsMade - idealConnections) * WeightConnections;
            float timePenalty = Mathf.Max(0, completionTime - idealTime) * WeightTime;
            float failuresPenalty = connectionsFailed * WeightFailures;
            float canceledPenalty = connectionsCanceled * WeightCanceled;
            float disconnectionsPenalty = disconnectionsCount * WeightDisconnections;

            float denominator = 1f
                + gatesPenalty
                + connectionsPenalty
                + timePenalty
                + failuresPenalty
                + canceledPenalty
                + disconnectionsPenalty;

            return ScoreBase / denominator;
        }
    }
}
