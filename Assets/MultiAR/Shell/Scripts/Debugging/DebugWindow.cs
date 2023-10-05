namespace MultiAR.Shell.Scripts.Debugging
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshPro debugText;

        private ScrollRect _scrollRect;

        private void Start()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();

            Application.logMessageReceived += HandleLog;
            debugText.text = "Debug messages will appear here.\n\n";
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            debugText.text += message + " \n";
            Canvas.ForceUpdateCanvases();
            if (_scrollRect)
            {
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }
    }
}
