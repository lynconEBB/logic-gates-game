using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// Reads player names from the Supabase Postgrest REST API.
    /// Uses the same anon-key/auth headers and async-request pattern as
    /// <see cref="TelemetryFirestoreSync"/>.
    /// </summary>
    public static class SupabasePlayersClient
    {
        [Serializable]
        private class PlayerRow
        {
            // Only the name is parsed. id/created_at are intentionally NOT
            // included: id may be a number in the DB, which would make
            // JsonUtility fail the whole parse on a number->string mismatch.
            public string name;
        }

        // JsonUtility cannot parse a top-level JSON array, so the response array
        // is wrapped into an object with this single field before parsing.
        [Serializable]
        private class PlayerRowList
        {
            public PlayerRow[] items;
        }

        public static async Task<List<string>> FetchLatestNamesAsync(SupabaseConfig config, int limit = 5)
        {
            if (config == null)
                throw new InvalidOperationException("Supabase config is not assigned.");

            if (string.IsNullOrWhiteSpace(config.ProjectUrl))
                throw new InvalidOperationException("Supabase project URL is not configured.");

            if (string.IsNullOrWhiteSpace(config.AnonKey))
                throw new InvalidOperationException("Supabase anon key is not configured.");

            string normalizedProjectUrl = config.ProjectUrl.TrimEnd('/');
            string requestUrl =
                $"{normalizedProjectUrl}/rest/v1/players?select=name&order=created_at.desc&limit={limit}";

            using var request = UnityWebRequest.Get(requestUrl);
            request.SetRequestHeader("apikey", config.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {config.AnonKey}");

            await SendRequestAsync(request);

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            Debug.Log($"[SupabasePlayersClient] GET {requestUrl} -> {request.responseCode}: {responseText}");

            if (!IsSuccess(request))
                throw new InvalidOperationException($"Supabase players query failed ({request.responseCode}): {responseText}");

            List<string> names = ParseNames(responseText);
            Debug.Log($"[SupabasePlayersClient] Parsed {names.Count} name(s).");
            return names;
        }

        private static List<string> ParseNames(string responseText)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(responseText))
                return names;

            PlayerRowList parsed = JsonUtility.FromJson<PlayerRowList>("{\"items\":" + responseText + "}");
            if (parsed?.items == null)
                return names;

            foreach (PlayerRow row in parsed.items)
            {
                if (row != null && !string.IsNullOrWhiteSpace(row.name))
                    names.Add(row.name);
            }

            return names;
        }

        private static async Task SendRequestAsync(UnityWebRequest request)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();
        }

        private static bool IsSuccess(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.Success &&
                request.responseCode >= 200 &&
                request.responseCode < 300;
        }
    }
}
