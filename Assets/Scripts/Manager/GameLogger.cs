using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

struct BufferInfo
{
    public string fileName;
    public string logFilePath;

    public BufferInfo(string fileName, string logFilePath)
    {
        this.fileName = fileName;
        this.logFilePath = logFilePath;
    }
};

struct BufferText
{
    public string logFilePath;
    public string format;

    public BufferText(string logFilePath, string format)
    {
        this.logFilePath = logFilePath;
        this.format = format;
    }
};

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    [SerializeField] private float flushInterval = 5.0f;        // 버퍼에 쌓인 로그를 파일에 기록할 시간 간격

    private string logFilePath;
    Dictionary<string, BufferInfo> logInfoes = new Dictionary<string, BufferInfo>();    // 로그 파일 정보
    List<BufferText> logBuffers = new List<BufferText>();                               // 로그 버퍼

    public string LogFilePath => logFilePath;

    private string _logDir;

    // 하위 로거
    public ClickLogger click {  get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 하위 로거 할당
        click = gameObject.GetComponent<ClickLogger>();

        string exeDir = Path.GetDirectoryName(Application.dataPath);
        string logDir = Path.Combine(exeDir, $"log\\{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
        _logDir = logDir;
        Directory.CreateDirectory(logDir);

        Log("Session", "=== Game Session Started ===");

        // 주기마다 파일에 로그 출력할 코루틴 시작
        StartCoroutine(FlushBufferRoutine());
    }

    // 파일에 로그 찍을 때, 호출하는 함수
    public void Log(string fileName, string message)
    {
        // 파일이 존재하지 않는 경우 추가
        if(logInfoes.ContainsKey(fileName) == false)
        {
            string fileNameTxt = $"{fileName}.txt";
            string filePath = Path.Combine(_logDir, fileNameTxt);
            logInfoes.Add(fileName, new BufferInfo(fileName, filePath));
        }

        string formatted = $"[{DateTime.Now:HH:mm:ss}] [{fileName}] {message}";
        Debug.Log(formatted); // 콘솔에는 출력

        // 데이터 추가
        lock (logBuffers)
        {
            logBuffers.Add(new BufferText(logInfoes[fileName].logFilePath, formatted));
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
        Log("Session", "=== Game Session End ===");
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
        if (logBuffers.Count == 0) return;

        // 로그 버퍼 내용을 모두 새롭게 복사
        List<BufferText> logsToWrite;
        lock(logBuffers)
        {
            logsToWrite = new List<BufferText>(logBuffers);
            logBuffers.Clear();
        }

        // 파일에 대입
        foreach(BufferText logEach in logsToWrite)
        {
            File.AppendAllText(logEach.logFilePath, logEach.format + Environment.NewLine);
        }
    }

}