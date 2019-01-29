using UberLogger;
using System.IO;
using UnityEngine;

public class UberLoggerFile : UberLogger.ILogger
{
    private StreamWriter LogFileWriter;
    private bool IncludeCallStacks;

    public UberLoggerFile(string filename, bool includeCallStacks = true)
    {
        IncludeCallStacks = includeCallStacks;
        var fileLogPath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        Debug.Log("Initialising file logging to " + fileLogPath);
        LogFileWriter = new StreamWriter(fileLogPath, false);
        LogFileWriter.AutoFlush = true;
    }

    public void Log(LogInfo logInfo)
    {
        lock(this)
        {
            LogFileWriter.WriteLine(logInfo.Message);
            if(IncludeCallStacks && logInfo.Callstack.Count>0)
            {
                foreach(var frame in logInfo.Callstack)
                {
                    LogFileWriter.WriteLine(frame.GetFormattedMethodNameWithFileName());
                }
                LogFileWriter.WriteLine();
            }
        }
    }
}
