using UnityEngine;
using System.Collections.Generic;

public class ConsoleToGUI : MonoBehaviour
{
    [SerializeField] private int logBoxWidth = 400;
    [SerializeField] private int logBoxHeight = 250;
    [SerializeField] private int maxLogs = 8;

    private List<string> logs = new List<string>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {    
        logs.Add(type.ToString() + ": " + logString);

        if (logs.Count > maxLogs)
        {
            logs.RemoveAt(0);
        }
    }

    void OnGUI()
    {
        // Calculate the position of the log box
        float x = 10;
        float y = Screen.height - logBoxHeight - 10;

        // Create a scroll view for logs
        GUI.TextArea(new Rect(x, y, logBoxWidth - 20, logBoxHeight -20), string.Join("\n", logs.ToArray()));
    }
}