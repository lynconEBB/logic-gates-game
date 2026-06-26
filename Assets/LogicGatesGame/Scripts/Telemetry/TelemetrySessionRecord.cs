using System;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [Serializable]
    public class TelemetrySessionRecord
    {
        public string sessionId;
        public string createdAtUtc;
        public string playerName;
        public int time;
        public string circuitExpression;
        public string appVersion;
        public string platform;
        public string deviceModel;

        public int gates;
        public int disconnections;
        public int connectionCanceled;
        public int connectionFailed;
        public int connectionSuccessful;
        public int connections;
        public float score;

        public string poseCsvRelativePath;
        public bool poseCsvAvailable;
        public string poseCsvError;
        public bool poseCsvUploaded;
        public string poseCsvStoragePath;
        public string poseCsvDownloadUrl;

        public static TelemetrySessionRecord Create(
            string sessionId,
            string createdAtUtc,
            string playerName,
            int time,
            string circuitExpression,
            int gates,
            int disconnections,
            int connectionCanceled,
            int connectionFailed,
            int connectionSuccessful,
            float score,
            TelemetryPoseCaptureResult poseCaptureResult)
        {
            if (poseCaptureResult == null)
                poseCaptureResult = TelemetryPoseCaptureResult.Unavailable(string.Empty);

            return new TelemetrySessionRecord
            {
                sessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId,
                createdAtUtc = string.IsNullOrWhiteSpace(createdAtUtc) ? DateTime.UtcNow.ToString("o") : createdAtUtc,
                playerName = playerName ?? string.Empty,
                time = time,
                circuitExpression = circuitExpression ?? string.Empty,
                appVersion = Application.version,
                platform = Application.platform.ToString(),
                deviceModel = SystemInfo.deviceModel,
                gates = gates,
                disconnections = disconnections,
                connectionCanceled = connectionCanceled,
                connectionFailed = connectionFailed,
                connectionSuccessful = connectionSuccessful,
                connections = connectionCanceled + connectionFailed + connectionSuccessful,
                score = score,
                poseCsvRelativePath = poseCaptureResult.poseCsvRelativePath ?? string.Empty,
                poseCsvAvailable = poseCaptureResult.poseCsvAvailable,
                poseCsvError = poseCaptureResult.poseCsvError ?? string.Empty,
                poseCsvUploaded = false,
                poseCsvStoragePath = string.Empty,
                poseCsvDownloadUrl = string.Empty
            };
        }
    }

    [Serializable]
    public class TelemetryPoseCaptureResult
    {
        public string poseCsvRelativePath;
        public bool poseCsvAvailable;
        public string poseCsvError;

        public static TelemetryPoseCaptureResult Available(string poseCsvRelativePath)
        {
            return new TelemetryPoseCaptureResult
            {
                poseCsvRelativePath = poseCsvRelativePath ?? string.Empty,
                poseCsvAvailable = true,
                poseCsvError = string.Empty
            };
        }

        public static TelemetryPoseCaptureResult Unavailable(string error)
        {
            return new TelemetryPoseCaptureResult
            {
                poseCsvRelativePath = string.Empty,
                poseCsvAvailable = false,
                poseCsvError = error ?? string.Empty
            };
        }
    }
}
