using System;
using System.IO;
using System.Globalization;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public static class TelemetryLocalStore
    {
        private const string TelemetryFolderName = "Telemetry";
        private const string PendingFolderName = "pending";
        private const string UploadedFolderName = "uploaded";
        private const string FileExtension = ".json";

        public static string TelemetryRootPath => Path.Combine(Application.persistentDataPath, TelemetryFolderName);
        public static string PendingFolderPath => Path.Combine(TelemetryRootPath, PendingFolderName);
        public static string UploadedFolderPath => Path.Combine(TelemetryRootPath, UploadedFolderName);

        public static void SaveCompletedSession(TelemetrySessionRecord record)
        {
            if (record == null)
            {
                Debug.LogWarning("[TelemetryLocalStore] Cannot save a null record.");
                return;
            }

            try
            {
                EnsureDirectoriesExist();

                string filePath = Path.Combine(PendingFolderPath, GetFileName(record));
                string json = JsonUtility.ToJson(record, true);
                File.WriteAllText(filePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to save telemetry session: {exception.Message}");
            }
        }

        public static string[] GetPendingSessionFilePaths()
        {
            try
            {
                EnsureDirectoriesExist();
                string[] filePaths = Directory.GetFiles(PendingFolderPath, $"*{FileExtension}", SearchOption.TopDirectoryOnly);
                Array.Sort(filePaths, StringComparer.Ordinal);
                return filePaths;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to enumerate pending telemetry files: {exception.Message}");
                return Array.Empty<string>();
            }
        }

        public static bool TryLoadSessionRecord(string filePath, out TelemetrySessionRecord record)
        {
            record = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Debug.LogWarning("[TelemetryLocalStore] Cannot load telemetry from an empty path.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning($"[TelemetryLocalStore] Telemetry file is empty: {filePath}");
                    return false;
                }

                record = JsonUtility.FromJson<TelemetrySessionRecord>(json);
                if (record == null)
                {
                    Debug.LogWarning($"[TelemetryLocalStore] Telemetry file could not be parsed: {filePath}");
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to load telemetry file '{filePath}': {exception.Message}");
                return false;
            }
        }

        public static bool ArchivePendingSessionFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Debug.LogWarning("[TelemetryLocalStore] Cannot archive telemetry from an empty path.");
                return false;
            }

            try
            {
                EnsureDirectoriesExist();

                string destinationPath = Path.Combine(UploadedFolderPath, Path.GetFileName(filePath));
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);

                File.Move(filePath, destinationPath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to archive telemetry file '{filePath}': {exception.Message}");
                return false;
            }
        }

        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(TelemetryRootPath);
            Directory.CreateDirectory(PendingFolderPath);
            Directory.CreateDirectory(UploadedFolderPath);
        }

        private static string GetFileName(TelemetrySessionRecord record)
        {
            DateTime completedAtUtc = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(record.completedAtUtc) &&
                DateTime.TryParse(record.completedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedCompletedAtUtc))
            {
                completedAtUtc = parsedCompletedAtUtc.ToUniversalTime();
            }

            string sessionId = string.IsNullOrWhiteSpace(record.sessionId) ? Guid.NewGuid().ToString("N") : record.sessionId;
            return $"{completedAtUtc:yyyyMMddTHHmmssZ}_{sessionId}{FileExtension}";
        }
    }
}
