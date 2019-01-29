#if !DevelopVersion && (DEVELOPMENT_BUILD || DEBUG || UNITY_EDITOR)
#define DevelopVersion
#endif

using UberLogger;

public static class UberDebug
{
    [StackTraceIgnore]
    static public void Log(UnityEngine.Object context, string message, params object[] par)
    {
        #if DevelopVersion
        UberLogger.Logger.Log("", context, LogSeverity.Message, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void Log(string message, params object[] par)
    {
        #if DevelopVersion
        UberLogger.Logger.Log("", null, LogSeverity.Message, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogChannel(UnityEngine.Object context, string channel, string message, params object[] par)
    {
        #if DevelopVersion
        UberLogger.Logger.Log(channel, context, LogSeverity.Message, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogChannel(string channel, string message, params object[] par)
    {
        #if DevelopVersion
        UberLogger.Logger.Log(channel, null, LogSeverity.Message, message, par);
        #endif
    }


    [StackTraceIgnore]
    static public void LogWarning(UnityEngine.Object context, object message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_WARNINGS)
        UberLogger.Logger.Log("", context, LogSeverity.Warning, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogWarning(object message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_WARNINGS)
        UberLogger.Logger.Log("", null, LogSeverity.Warning, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogWarningChannel(UnityEngine.Object context, string channel, string message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_WARNINGS)
        UberLogger.Logger.Log(channel, context, LogSeverity.Warning, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogWarningChannel(string channel, string message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_WARNINGS)
        UberLogger.Logger.Log(channel, null, LogSeverity.Warning, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogError(UnityEngine.Object context, object message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_ERRORS)
        UberLogger.Logger.Log("", context, LogSeverity.Error, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogError(object message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_ERRORS)
        UberLogger.Logger.Log("", null, LogSeverity.Error, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogErrorChannel(UnityEngine.Object context, string channel, string message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_ERRORS)
        UberLogger.Logger.Log(channel, context, LogSeverity.Error, message, par);
        #endif
    }

    [StackTraceIgnore]
    static public void LogErrorChannel(string channel, string message, params object[] par)
    {
        #if (DevelopVersion || DevelopVersion_ERRORS)
        UberLogger.Logger.Log(channel, null, LogSeverity.Error, message, par);
        #endif
    }

    [LogUnityOnly]
    static public void UnityLog(object message)
    {
        #if DevelopVersion
        UnityEngine.Debug.Log(message);
        #endif
    }

    [LogUnityOnly]
    static public void UnityLogWarning(object message)
    {
        #if (DevelopVersion || DevelopVersion_WARNINGS)
        UnityEngine.Debug.LogWarning(message);
        #endif
    }

    [LogUnityOnly]
    static public void UnityLogError(object message)
    {
        #if (DevelopVersion || DevelopVersion_ERRORS)
        UnityEngine.Debug.LogError(message);
        #endif
    }
}
