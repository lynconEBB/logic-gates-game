using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class TelemetryFirestoreSync : Singleton<TelemetryFirestoreSync>
    {
        [SerializeField] private string collectionName = "telemetrySessions";
        [SerializeField] private bool syncOnStart = true;
        [SerializeField] private bool syncOnResume = true;

        private FirebaseFirestore _firestore;
        private bool _firebaseReady;
        private bool _syncInProgress;

        protected override void Init()
        {
            InitializeFirebase();
        }

        private void Start()
        {
            if (syncOnStart)
                TrySyncPendingSessions();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && syncOnResume)
                TrySyncPendingSessions();
        }

        public void TrySyncPendingSessions()
        {
            if (_syncInProgress)
                return;

            if (!_firebaseReady)
            {
                Debug.Log("[TelemetryFirestoreSync] Firebase is not ready yet. Sync will be retried later.");
                return;
            }

            if (!HasNetworkConnection())
            {
                Debug.Log("[TelemetryFirestoreSync] No network connection. Pending telemetry remains queued locally.");
                return;
            }

            SyncPendingSessionsAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogWarning($"[TelemetryFirestoreSync] Sync failed: {task.Exception}");
            });
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[TelemetryFirestoreSync] Firebase dependency check failed: {task.Exception}");
                    return;
                }

                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError($"[TelemetryFirestoreSync] Firebase dependencies are not available: {task.Result}");
                    return;
                }

                _firestore = FirebaseFirestore.DefaultInstance;
                _firebaseReady = _firestore != null;

                if (!_firebaseReady)
                {
                    Debug.LogError("[TelemetryFirestoreSync] Firebase Firestore instance was not created.");
                    return;
                }

                Debug.Log("[TelemetryFirestoreSync] Firebase initialized successfully.");

                if (syncOnStart)
                    TrySyncPendingSessions();
            });
        }

        private async Task SyncPendingSessionsAsync()
        {
            _syncInProgress = true;
            try
            {
                string[] pendingFilePaths = TelemetryLocalStore.GetPendingSessionFilePaths();

                foreach (string filePath in pendingFilePaths)
                {
                    if (!TelemetryLocalStore.TryLoadSessionRecord(filePath, out TelemetrySessionRecord session))
                    {
                        Debug.LogWarning($"[TelemetryFirestoreSync] Stopping sync because a telemetry file could not be read: {filePath}");
                        break;
                    }

                    await UploadSessionAsync(session);
                    if (!TelemetryLocalStore.ArchivePendingSessionFile(filePath))
                        break;
                }
            }
            finally
            {
                _syncInProgress = false;
            }
        }

        private Task UploadSessionAsync(TelemetrySessionRecord session)
        {
            var documentData = new Dictionary<string, object>
            {
                { "sessionId", session.sessionId },
                { "createdAtUtc", session.createdAtUtc },
                { "completedAtUtc", session.completedAtUtc },
                { "elapsedSeconds", session.elapsedSeconds },
                { "sceneName", session.sceneName },
                { "circuitExpression", session.circuitExpression },
                { "appVersion", session.appVersion },
                { "platform", session.platform },
                { "deviceModel", session.deviceModel },
                { "uploadedToFirestore", true },
                { "counters", BuildCountersMap(session.counters) }
            };

            return _firestore.Collection(collectionName).Document(session.sessionId).SetAsync(documentData);
        }

        private static Dictionary<string, object> BuildCountersMap(List<TelemetryCounterEntry> counters)
        {
            var map = new Dictionary<string, object>();
            if (counters == null)
                return map;

            foreach (TelemetryCounterEntry counter in counters)
            {
                if (counter == null || string.IsNullOrEmpty(counter.key))
                    continue;

                map[counter.key] = counter.value;
            }

            return map;
        }

        private static bool HasNetworkConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}
