using System;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [Serializable]
    public class TelemetrySessionRecord
    {
        public string sessionId;
        public string createdAtUtc;
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

        public static TelemetrySessionRecord Create(
            int time,
            string circuitExpression,
            int gates,
            int disconnections,
            int connectionCanceled,
            int connectionFailed,
            int connectionSuccessful,
            float score)
        {
            return new TelemetrySessionRecord
            {
                sessionId = Guid.NewGuid().ToString("N"),
                createdAtUtc = DateTime.UtcNow.ToString("o"),
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
                score = score
            };
        }
    }
}
