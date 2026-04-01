using TMPro;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text gatesCountText;
        [SerializeField] private TMP_Text connectionsCountText;

        private void OnEnable()
        {
            if (timeText != null)
            {
                int totalSeconds = GameManager.Instance != null ? GameManager.Instance.ElapsedSeconds : 0;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            var telemetry = TelemetryManager.Instance;
            if (gatesCountText != null)
                gatesCountText.text = (telemetry != null ? telemetry.GetCount(CircuitTelemetryManager.KeyGates) : 0).ToString();
            if (connectionsCountText != null)
                connectionsCountText.text = (telemetry != null ? telemetry.GetCount(CircuitTelemetryManager.KeyConnections) : 0).ToString();
        }
    }
}
