using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    [SerializeField] private float flushInterval = 5.0f;        // 버퍼에 쌓인 로그를 파일에 기록할 시간 간격

    private string logFilePath;
    private List<string> logBuffer = new List<string>();        // 로그 버퍼

    public string LogFilePath => logFilePath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string exeDir = Path.GetDirectoryName(Application.dataPath);
        string logDir = Path.Combine(exeDir, $"log\\{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
        Directory.CreateDirectory(logDir);
        string fileName = $"GameLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        logFilePath = Path.Combine(logDir, fileName);

        // 유니티 로그만 처리
        // Application.logMessageReceived += HandleUnityLog;

        Log("=== Game Session Started ===");

        // 주기마다 파일에 로그 출력할 코루틴 시작
        StartCoroutine(FlushBufferRoutine());
    }

    // 파일에 로그 찍을 때, 호출하는 함수
    public void Log(string message)
    {
        string formatted = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Debug.Log(formatted); // 콘솔에는 출력

        lock (logBuffer)
        {
            logBuffer.Add(formatted);
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        // 직접 출력한 로그는 이미 파일에 썼으므로, 중복 방지
        if (logString.StartsWith("["))
            return;

        string formatted = $"[{DateTime.Now:HH:mm:ss}] [{type}] {logString}";
        if (type == LogType.Error || type == LogType.Exception)
            formatted += $"\\n{stackTrace}";

        File.AppendAllText(logFilePath, formatted + Environment.NewLine);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    // 게임이 종료될 때, 남은 로그 모두 출력
    private void OnDisable()
    {
        FlushLogsToFile();
    }

    // 주기적으로 로그 버퍼에 있는 내용 모두 파일에 출력
    IEnumerator FlushBufferRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(flushInterval);

            FlushLogsToFile();
        }
    }

    // 버퍼에 있는 로그 모두 출력
    void FlushLogsToFile()
    {
        if (logBuffer.Count == 0) return;

        // 로그 버퍼 내용을 모두 새롭게 복사
        List<string> logsToWrite;
        lock(logBuffer)
        {
            logsToWrite = new List<string>(logBuffer);
            logBuffer.Clear();
        }

        // 파일에 대입
        foreach(string logString in logsToWrite)
        {
            File.AppendAllText(logFilePath, logString + Environment.NewLine);
        }
    }

}