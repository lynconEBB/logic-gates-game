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
        private const string PosePendingFolderName = "pose_pending";
        private const string PoseUploadedFolderName = "pose_uploaded";
        private const string FileExtension = ".json";

        public static string TelemetryRootPath => Path.Combine(Application.persistentDataPath, TelemetryFolderName);
        public static string PendingFolderPath => Path.Combine(TelemetryRootPath, PendingFolderName);
        public static string UploadedFolderPath => Path.Combine(TelemetryRootPath, UploadedFolderName);
        public static string PosePendingFolderPath => Path.Combine(TelemetryRootPath, PosePendingFolderName);
        public static string PoseUploadedFolderPath => Path.Combine(TelemetryRootPath, PoseUploadedFolderName);

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

        public static bool UpdateSessionFile(string filePath, TelemetrySessionRecord record)
        {
            if (record == null)
            {
                Debug.LogWarning("[TelemetryLocalStore] Cannot update a telemetry file with a null record.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Debug.LogWarning("[TelemetryLocalStore] Cannot update telemetry at an empty path.");
                return false;
            }

            try
            {
                EnsureDirectoriesExist();
                string json = JsonUtility.ToJson(record, true);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to update telemetry file '{filePath}': {exception.Message}");
                return false;
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

        public static string GetPoseCsvRelativePath(string sessionId)
        {
            string safeSessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId;
            return $"{PosePendingFolderName}/{safeSessionId}_poses.csv";
        }

        public static string ResolveTelemetryPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            string normalizedRelativePath = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(TelemetryRootPath, normalizedRelativePath);
        }

        public static bool TryGetPoseCsvPath(TelemetrySessionRecord record, out string filePath)
        {
            filePath = string.Empty;
            if (record == null || string.IsNullOrWhiteSpace(record.poseCsvRelativePath))
                return false;

            filePath = ResolveTelemetryPath(record.poseCsvRelativePath);
            return !string.IsNullOrWhiteSpace(filePath);
        }

        public static bool ArchivePendingPoseCsv(TelemetrySessionRecord record)
        {
            if (record == null || !record.poseCsvAvailable || string.IsNullOrWhiteSpace(record.poseCsvRelativePath))
                return true;

            try
            {
                EnsureDirectoriesExist();

                if (record.poseCsvRelativePath.StartsWith($"{PoseUploadedFolderName}/", StringComparison.OrdinalIgnoreCase))
                    return true;

                string sourcePath = ResolveTelemetryPath(record.poseCsvRelativePath);
                string fileName = Path.GetFileName(sourcePath);
                string uploadedRelativePath = $"{PoseUploadedFolderName}/{fileName}";
                string destinationPath = Path.Combine(PoseUploadedFolderPath, fileName);

                if (!File.Exists(sourcePath))
                {
                    if (File.Exists(destinationPath))
                    {
                        record.poseCsvRelativePath = uploadedRelativePath;
                        return true;
                    }

                    Debug.LogWarning($"[TelemetryLocalStore] Cannot archive missing pose CSV: {sourcePath}");
                    return false;
                }

                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);

                File.Move(sourcePath, destinationPath);
                record.poseCsvRelativePath = uploadedRelativePath;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[TelemetryLocalStore] Failed to archive pose CSV: {exception.Message}");
                return false;
            }
        }

        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(TelemetryRootPath);
            Directory.CreateDirectory(PendingFolderPath);
            Directory.CreateDirectory(UploadedFolderPath);
            Directory.CreateDirectory(PosePendingFolderPath);
            Directory.CreateDirectory(PoseUploadedFolderPath);
        }

        private static string GetFileName(TelemetrySessionRecord record)
        {
            DateTime createdAtUtc = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(record.createdAtUtc) &&
                DateTime.TryParse(record.createdAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedCreatedAtUtc))
            {
                createdAtUtc = parsedCreatedAtUtc.ToUniversalTime();
            }

            string sessionId = string.IsNullOrWhiteSpace(record.sessionId) ? Guid.NewGuid().ToString("N") : record.sessionId;
            return $"{createdAtUtc:yyyyMMddTHHmmssZ}_{sessionId}{FileExtension}";
        }
    }
}
