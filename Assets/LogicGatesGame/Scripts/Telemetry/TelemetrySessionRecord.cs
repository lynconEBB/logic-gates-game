using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [Serializable]
    public class TelemetryCounterEntry
    {
        public string key;
        public int value;
    }

    [Serializable]
    public class TelemetrySessionRecord
    {
        public string sessionId;
        public string createdAtUtc;
        public string completedAtUtc;
        public int elapsedSeconds;
        public string sceneName;
        public string circuitExpression;
        public string appVersion;
        public string platform;
        public string deviceModel;
        public List<TelemetryCounterEntry> counters = new List<TelemetryCounterEntry>();

        public static TelemetrySessionRecord Create(
            int elapsedSeconds,
            string sceneName,
            string circuitExpression,
            IReadOnlyDictionary<string, int> telemetryValues)
        {
            var record = new TelemetrySessionRecord
            {
                sessionId = Guid.NewGuid().ToString("N"),
                createdAtUtc = DateTime.UtcNow.ToString("o"),
                completedAtUtc = DateTime.UtcNow.ToString("o"),
                elapsedSeconds = elapsedSeconds,
                sceneName = sceneName,
                circuitExpression = circuitExpression ?? string.Empty,
                appVersion = Application.version,
                platform = Application.platform.ToString(),
                deviceModel = SystemInfo.deviceModel
            };

            if (telemetryValues == null)
                return record;

            foreach (var entry in telemetryValues)
            {
                record.counters.Add(new TelemetryCounterEntry
                {
                    key = entry.Key,
                    value = entry.Value
                });
            }

            return record;
        }
    }
}
