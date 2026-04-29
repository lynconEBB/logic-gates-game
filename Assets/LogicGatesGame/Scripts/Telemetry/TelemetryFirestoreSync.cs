using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Networking;

namespace LogicGatesGame.Scripts
{
    public class TelemetryFirestoreSync : Singleton<TelemetryFirestoreSync>
    {
        [SerializeField] private string collectionName = "telemetrySessions";
        [SerializeField] private string poseCsvStorageFolder = "telemetryPoseCsv";
        [SerializeField] private string supabaseProjectUrl;
        [SerializeField] private string supabaseAnonKey;
        [SerializeField] private string supabaseBucketName = "telemetry-pose-csv";
        [SerializeField] private bool syncOnStart = true;
        [SerializeField] private bool syncOnResume = true;

        private FirebaseFirestore _firestore;
        private bool _firebaseReady;
        private bool _syncInProgress;

        [ContextMenu("Test Supabase CSV Upload")]
        private async void TestSupabaseCsvUpload()
        {
            try
            {
                string testFilePath = Path.Combine(Application.temporaryCachePath, "supabase_upload_test.csv");
                string timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                File.WriteAllText(testFilePath, $"kind,timestampUtc{Environment.NewLine}supabase-upload-test,{timestamp}{Environment.NewLine}");

                string storagePath = $"{poseCsvStorageFolder}/manual_test_{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}.csv";
                string publicUrl = await UploadFileToStorageAsync(testFilePath, storagePath);

                Debug.Log($"[TelemetryFirestoreSync] Supabase test upload succeeded: {publicUrl}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryFirestoreSync] Supabase test upload failed: {exception.Message}");
            }
        }

        protected override void Init()
        {
            InitializeFirebase();
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

                Debug.Log("[TelemetryFirestoreSync] Firebase Firestore initialized successfully.");

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

                    await UploadSessionAsync(filePath, session);
                    if (!TelemetryLocalStore.ArchivePendingPoseCsv(session))
                        break;

                    if (!TelemetryLocalStore.UpdateSessionFile(filePath, session))
                        break;

                    if (!TelemetryLocalStore.ArchivePendingSessionFile(filePath))
                        break;
                }
            }
            finally
            {
                _syncInProgress = false;
            }
        }

        private async Task UploadSessionAsync(string sessionFilePath, TelemetrySessionRecord session)
        {
            await UploadPoseCsvIfNeededAsync(session);
            if (!TelemetryLocalStore.UpdateSessionFile(sessionFilePath, session))
                throw new IOException($"Failed to persist telemetry upload state before Firestore sync: {sessionFilePath}");

            Dictionary<string, object> documentData = CreateDocumentData(session);
            await _firestore.Collection(collectionName).Document(session.sessionId).SetAsync(documentData);
        }

        private Dictionary<string, object> CreateDocumentData(TelemetrySessionRecord session)
        {
            var documentData = new Dictionary<string, object>
            {
                { "sessionId", session.sessionId },
                { "createdAtUtc", session.createdAtUtc },
                { "time", session.time },
                { "circuitExpression", session.circuitExpression },
                { "appVersion", session.appVersion },
                { "platform", session.platform },
                { "deviceModel", session.deviceModel },
                { "gates", session.gates },
                { "disconnections", session.disconnections },
                { "connectionCanceled", session.connectionCanceled },
                { "connectionFailed", session.connectionFailed },
                { "connectionSuccessful", session.connectionSuccessful },
                { "connections", session.connections },
                { "score", session.score },
                { "poseCsvDownloadUrl", session.poseCsvDownloadUrl ?? string.Empty },
                { "uploadedToFirestore", true }
            };

            return documentData;
        }

        private async Task UploadPoseCsvIfNeededAsync(TelemetrySessionRecord session)
        {
            if (session == null || !session.poseCsvAvailable)
                return;

            if (session.poseCsvUploaded && !string.IsNullOrWhiteSpace(session.poseCsvDownloadUrl))
                return;

            if (!TelemetryLocalStore.TryGetPoseCsvPath(session, out string poseCsvPath) || !File.Exists(poseCsvPath))
            {
                session.poseCsvAvailable = false;
                session.poseCsvError = "Pose CSV file was not found during sync.";
                return;
            }

            if (string.IsNullOrWhiteSpace(session.poseCsvStoragePath))
                session.poseCsvStoragePath = $"{poseCsvStorageFolder}/{session.sessionId}.csv";

            string downloadUrl = await UploadFileToStorageAsync(poseCsvPath, session.poseCsvStoragePath);
            session.poseCsvUploaded = true;
            session.poseCsvDownloadUrl = downloadUrl ?? string.Empty;
        }

        private async Task<string> UploadFileToStorageAsync(string filePath, string storagePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Pose CSV file was not found before upload.", filePath);

            if (string.IsNullOrWhiteSpace(supabaseProjectUrl))
                throw new InvalidOperationException("Supabase project URL is not configured.");

            if (string.IsNullOrWhiteSpace(supabaseAnonKey))
                throw new InvalidOperationException("Supabase anon key is not configured.");

            if (string.IsNullOrWhiteSpace(supabaseBucketName))
                throw new InvalidOperationException("Supabase bucket name is not configured.");

            byte[] csvBytes = File.ReadAllBytes(filePath);
            string normalizedProjectUrl = supabaseProjectUrl.TrimEnd('/');
            string normalizedStoragePath = NormalizeStoragePath(storagePath);
            string uploadUrl = $"{normalizedProjectUrl}/storage/v1/object/{UnityWebRequest.EscapeURL(supabaseBucketName)}/{EscapeStoragePath(normalizedStoragePath)}";
            string publicUrl = $"{normalizedProjectUrl}/storage/v1/object/public/{UnityWebRequest.EscapeURL(supabaseBucketName)}/{EscapeStoragePath(normalizedStoragePath)}";

            using var request = new UnityWebRequest(uploadUrl, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(csvBytes),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            request.SetRequestHeader("Content-Type", "text/csv");
            request.SetRequestHeader("x-upsert", "false");

            await SendRequestAsync(request);

            if (IsUploadSuccess(request))
                return publicUrl;

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (IsAlreadyExistsResponse(request.responseCode, responseText))
                return publicUrl;

            throw new InvalidOperationException($"Supabase Storage upload failed ({request.responseCode}): {responseText}");
        }

        private static async Task SendRequestAsync(UnityWebRequest request)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();
        }

        private static bool IsUploadSuccess(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.Success &&
                request.responseCode >= 200 &&
                request.responseCode < 300;
        }

        private static bool IsAlreadyExistsResponse(long responseCode, string responseText)
        {
            return responseCode == 400 &&
                !string.IsNullOrWhiteSpace(responseText) &&
                responseText.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeStoragePath(string storagePath)
        {
            return (storagePath ?? string.Empty).Replace('\\', '/').TrimStart('/');
        }

        private static string EscapeStoragePath(string storagePath)
        {
            string[] segments = NormalizeStoragePath(storagePath).Split('/');
            for (int i = 0; i < segments.Length; i++)
                segments[i] = UnityWebRequest.EscapeURL(segments[i]);

            return string.Join("/", segments);
        }

        private static bool HasNetworkConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}
