using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class TelemetryPoseRecorder : MonoBehaviour
    {
        private const string CsvHeader =
            "sessionId,sampleIndex,elapsedSeconds," +
            "headPosX,headPosY,headPosZ,headRotX,headRotY,headRotZ,headRotW," +
            "leftPosX,leftPosY,leftPosZ,leftRotX,leftRotY,leftRotZ,leftRotW," +
            "rightPosX,rightPosY,rightPosZ,rightRotX,rightRotY,rightRotZ,rightRotW";

        [SerializeField] private GameManager gameManager;
        [SerializeField] private Transform xrOrigin;
        [SerializeField, Min(0.01f)] private float sampleIntervalSeconds = 0.1f;
        [SerializeField, Min(1)] private int flushSampleCount = 600;

        private readonly List<PoseSample> _activeBuffer = new List<PoseSample>(600);
        private readonly List<PoseSample> _writeBuffer = new List<PoseSample>(600);

        private Transform _head;
        private Transform _leftController;
        private Transform _rightController;
        private string _sessionId;
        private string _csvRelativePath;
        private string _csvAbsolutePath;
        private string _csvError;
        private bool _csvAvailable;
        private bool _recording;
        private bool _completed;
        private float _captureStartTime;
        private float _nextSampleTime;
        private int _sampleIndex;
        private Task _writeTask = Task.CompletedTask;

        private void Start()
        {
            ResolveReferences();
            StartRecording();
        }

        private void Update()
        {
            ObserveCompletedWrite();

            if (!_recording || !_csvAvailable)
                return;

            if (Time.time >= _nextSampleTime)
            {
                CaptureSample(Time.time - _captureStartTime);
                _nextSampleTime = Time.time + sampleIntervalSeconds;
            }
        }

        public async Task<TelemetryPoseCaptureResult> CompleteRecordingAsync()
        {
            if (_completed)
                return GetCaptureResult();

            _completed = true;
            _recording = false;

            if (!_csvAvailable)
                return GetCaptureResult();

            try
            {
                await AwaitCurrentWriteAsync();

                if (_activeBuffer.Count > 0)
                {
                    SwapBuffersForWrite();
                    await AwaitCurrentWriteAsync();
                }
            }
            catch (Exception exception)
            {
                DisableCsvRecording(exception.Message);
            }

            return GetCaptureResult();
        }

        public TelemetryPoseCaptureResult GetCaptureResult()
        {
            return _csvAvailable
                ? TelemetryPoseCaptureResult.Available(_csvRelativePath)
                : TelemetryPoseCaptureResult.Unavailable(_csvError);
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
                gameManager = GetComponent<GameManager>();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();

            Camera mainCamera = Camera.main;
            _head = mainCamera != null ? mainCamera.transform : null;

            if (xrOrigin == null)
                xrOrigin = FindTransformByNameContains("XR Origin");

            if (xrOrigin != null)
            {
                _leftController = FindChildByNormalizedName(xrOrigin, "leftcontroller");
                _rightController = FindChildByNormalizedName(xrOrigin, "rightcontroller");
            }
        }

        private void StartRecording()
        {
            TelemetryManager telemetryManager = TelemetryManager.Instance;
            if (telemetryManager == null)
            {
                DisableCsvRecording("TelemetryManager was not found.");
                return;
            }

            telemetryManager.EnsureSessionInitialized();
            _sessionId = telemetryManager.SessionId;
            _csvRelativePath = TelemetryLocalStore.GetPoseCsvRelativePath(_sessionId);
            _csvAbsolutePath = TelemetryLocalStore.ResolveTelemetryPath(_csvRelativePath);

            try
            {
                string directory = Path.GetDirectoryName(_csvAbsolutePath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(_csvAbsolutePath, CsvHeader + Environment.NewLine);
                _csvAvailable = true;
            }
            catch (Exception exception)
            {
                DisableCsvRecording(exception.Message);
                return;
            }

            _activeBuffer.Capacity = Mathf.Max(_activeBuffer.Capacity, flushSampleCount);
            _writeBuffer.Capacity = Mathf.Max(_writeBuffer.Capacity, flushSampleCount);
            _captureStartTime = Time.time;
            _nextSampleTime = _captureStartTime + sampleIntervalSeconds;
            _sampleIndex = 0;
            _recording = true;

            CaptureSample(0f);
        }

        private void CaptureSample(float elapsedSeconds)
        {
            var sample = new PoseSample
            {
                sessionId = _sessionId,
                sampleIndex = _sampleIndex,
                elapsedSeconds = elapsedSeconds,
                head = CapturePose(_head),
                leftController = CapturePose(_leftController),
                rightController = CapturePose(_rightController)
            };

            _activeBuffer.Add(sample);
            _sampleIndex++;

            if (_activeBuffer.Count >= flushSampleCount && _writeTask.IsCompleted)
                SwapBuffersForWrite();
        }

        private static PoseSnapshot CapturePose(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                return PoseSnapshot.Unavailable();

            Vector3 position = target.position;
            Quaternion rotation = target.rotation;
            return new PoseSnapshot
            {
                hasPose = true,
                px = position.x,
                py = position.y,
                pz = position.z,
                rx = rotation.x,
                ry = rotation.y,
                rz = rotation.z,
                rw = rotation.w
            };
        }

        private void SwapBuffersForWrite()
        {
            if (_activeBuffer.Count == 0)
                return;

            _writeBuffer.Clear();
            _writeBuffer.AddRange(_activeBuffer);
            _activeBuffer.Clear();

            PoseSample[] samples = _writeBuffer.ToArray();
            _writeTask = AppendSamplesAsync(samples);
        }

        private async Task AwaitCurrentWriteAsync()
        {
            if (_writeTask != null)
                await _writeTask;
        }

        private void ObserveCompletedWrite()
        {
            if (!_csvAvailable || _writeTask == null || !_writeTask.IsFaulted)
                return;

            DisableCsvRecording(_writeTask.Exception != null ? _writeTask.Exception.GetBaseException().Message : "CSV write failed.");
        }

        private Task AppendSamplesAsync(PoseSample[] samples)
        {
            string filePath = _csvAbsolutePath;
            return Task.Run(() =>
            {
                var builder = new StringBuilder(samples.Length * 256);
                foreach (PoseSample sample in samples)
                    sample.AppendCsvLine(builder);

                File.AppendAllText(filePath, builder.ToString());
            });
        }

        private void DisableCsvRecording(string error)
        {
            _recording = false;
            _csvAvailable = false;
            _csvError = error ?? string.Empty;
            Debug.LogWarning($"[TelemetryPoseRecorder] Pose CSV recording disabled: {_csvError}");
        }

        private static Transform FindTransformByNameContains(string namePart)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;
            }

            return null;
        }

        private static Transform FindChildByNormalizedName(Transform root, string normalizedName)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string childName = child.name.Replace(" ", string.Empty).ToLowerInvariant();
                if (childName == normalizedName)
                    return child;
            }

            return null;
        }

        private struct PoseSample
        {
            public string sessionId;
            public int sampleIndex;
            public float elapsedSeconds;
            public PoseSnapshot head;
            public PoseSnapshot leftController;
            public PoseSnapshot rightController;

            public void AppendCsvLine(StringBuilder builder)
            {
                builder.Append(sessionId);
                builder.Append(',');
                builder.Append(sampleIndex.ToString(CultureInfo.InvariantCulture));
                builder.Append(',');
                builder.Append(elapsedSeconds.ToString("F3", CultureInfo.InvariantCulture));
                head.AppendCsvColumns(builder);
                leftController.AppendCsvColumns(builder);
                rightController.AppendCsvColumns(builder);
                builder.AppendLine();
            }
        }

        private struct PoseSnapshot
        {
            public bool hasPose;
            public float px;
            public float py;
            public float pz;
            public float rx;
            public float ry;
            public float rz;
            public float rw;

            public static PoseSnapshot Unavailable()
            {
                return new PoseSnapshot { hasPose = false };
            }

            public void AppendCsvColumns(StringBuilder builder)
            {
                if (!hasPose)
                {
                    builder.Append(",,,,,,,");
                    return;
                }

                AppendValue(builder, px);
                AppendValue(builder, py);
                AppendValue(builder, pz);
                AppendValue(builder, rx);
                AppendValue(builder, ry);
                AppendValue(builder, rz);
                AppendValue(builder, rw);
            }

            private static void AppendValue(StringBuilder builder, float value)
            {
                builder.Append(',');
                builder.Append(value.ToString("F3", CultureInfo.InvariantCulture));
            }
        }
    }
}
