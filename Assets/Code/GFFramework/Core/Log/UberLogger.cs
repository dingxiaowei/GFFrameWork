using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace UberLogger
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StackTraceIgnore : Attribute {}

    [AttributeUsage(AttributeTargets.Method)]
    public class LogUnityOnly : Attribute {}

    public enum LogSeverity
    {
        Message,
        Warning,
        Error,
    }

    public interface ILogger
    {
        void Log(LogInfo logInfo);
    }

    public interface IFilter
    {
        bool ApplyFilter(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par);
    }

    [System.Serializable]
    public class LogStackFrame
    {
        public string MethodName;
        public string DeclaringType;
        public string ParameterSig;

        public int LineNumber;
        public string FileName;

        string FormattedMethodNameWithFileName;
        string FormattedMethodName;
        string FormattedFileName;

        public LogStackFrame(StackFrame frame)
        {
            var method = frame.GetMethod();
            MethodName = method.Name;
            DeclaringType = method.DeclaringType.FullName;

            var pars = method.GetParameters();
            for (int c1=0; c1<pars.Length; c1++)
            {
                ParameterSig += String.Format("{0} {1}", pars[c1].ParameterType, pars[c1].Name);
                if(c1+1<pars.Length)
                {
                    ParameterSig += ", ";
                }
            }

            FileName = frame.GetFileName();
            LineNumber = frame.GetFileLineNumber();
            MakeFormattedNames();
        }

        public LogStackFrame(string unityStackFrame)
        {
            if(Logger.ExtractInfoFromUnityStackInfo(unityStackFrame, ref DeclaringType, ref MethodName, ref FileName, ref LineNumber))
            {
                MakeFormattedNames();
            }
            else
            {
                FormattedMethodNameWithFileName = unityStackFrame;
                FormattedMethodName = unityStackFrame;
                FormattedFileName = unityStackFrame;
            }
        }

        public LogStackFrame(string message, string filename, int lineNumber)
        {
            FileName = filename;
            LineNumber = lineNumber;
            FormattedMethodNameWithFileName = message;
            FormattedMethodName = message;
            FormattedFileName = message;
        }

        public string GetFormattedMethodNameWithFileName()
        {
            return FormattedMethodNameWithFileName;
        }

        public string GetFormattedMethodName()
        {
            return FormattedMethodName;
        }

        public string GetFormattedFileName()
        {
            return FormattedFileName;
        }

        void MakeFormattedNames()
        {
            FormattedMethodName = String.Format("{0}.{1}({2})", DeclaringType, MethodName, ParameterSig);

            string filename = FileName;
            if(!String.IsNullOrEmpty(FileName))
            {
                var startSubName = FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

                if(startSubName>0)
                {
                    filename = FileName.Substring(startSubName);
                }
            }
            FormattedFileName = String.Format("{0}:{1}", filename, LineNumber);

            FormattedMethodNameWithFileName = String.Format("{0} (at {1})", FormattedMethodName, FormattedFileName);
        }
    }

    [System.Serializable]
    public class LogInfo
    {
        public UnityEngine.Object Source;
        public string Channel;
        public LogSeverity Severity;
        public string Message;
        public List<LogStackFrame> Callstack;
        public LogStackFrame OriginatingSourceLocation;
        public double RelativeTimeStamp;
        string RelativeTimeStampAsString;
        public DateTime AbsoluteTimeStamp;
        string AbsoluteTimeStampAsString;

        public string GetRelativeTimeStampAsString()
        {
            return RelativeTimeStampAsString;
        }

        public string GetAbsoluteTimeStampAsString()
        {
            return AbsoluteTimeStampAsString;
        }

        public LogInfo(UnityEngine.Object source, string channel, LogSeverity severity, List<LogStackFrame> callstack, LogStackFrame originatingSourceLocation, object message, params object[] par)
        {
            Source = source;
            Channel = channel;
            Severity = severity;
            Message = "";
            OriginatingSourceLocation = originatingSourceLocation;

            var messageString = message as String;
            if(messageString!=null)
            {
                if(par.Length>0)
                {
                    Message = System.String.Format(messageString, par);
                }
                else
                {
                    Message = messageString;
                }
            }
            else
            {
                if(message!=null)
                {
                    Message = message.ToString();
                }
            }

            Callstack = callstack;
            RelativeTimeStamp = Logger.GetRelativeTime();
            AbsoluteTimeStamp = DateTime.UtcNow;
            RelativeTimeStampAsString = String.Format("{0:0.0000}", RelativeTimeStamp);
            AbsoluteTimeStampAsString = AbsoluteTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public static class Logger
    {
        public static int MaxMessagesToKeep = 1000;

        public static bool ForwardMessages = true;

        public static string UnityInternalNewLine = "\n";

        public static char UnityInternalDirectorySeparator = '/';

        static List<ILogger> Loggers = new List<ILogger>();
        static LinkedList<LogInfo> RecentMessages = new LinkedList<LogInfo>();
        static long StartTick;
        static bool AlreadyLogging = false;
        static Regex UnityMessageRegex;
        static List<IFilter> Filters = new List<IFilter>();
        
        static Logger()
        {
#if UNITY_5 || UNITY_5_3_OR_NEWER
            Application.logMessageReceivedThreaded += UnityLogHandler;
#else
            Application.RegisterLogCallback(UnityLogHandler);
#endif
            StartTick = DateTime.Now.Ticks;
            UnityMessageRegex = new Regex(@"(.*)\((\d+).*\)");
        }

        /// <summary>
        /// Registered Unity error handler
        /// </summary>
        [StackTraceIgnore]
        static void UnityLogHandler(string logString, string stackTrace, UnityEngine.LogType logType)
        {
            UnityLogInternal(logString, stackTrace, logType);
        }
    
        static public double GetRelativeTime()
        {
            long ticks = DateTime.Now.Ticks;
            return TimeSpan.FromTicks(ticks - StartTick).TotalSeconds;
        }

        static public void AddLogger(ILogger logger, bool populateWithExistingMessages=true)
        {
            lock(Loggers)
            {
                if(populateWithExistingMessages)
                {
                    foreach(var oldLog in RecentMessages)
                    {
                        logger.Log(oldLog);
                    }
                }

                if(!Loggers.Contains(logger))
                {
                    Loggers.Add(logger);
                }
            }
        }

        static public void AddFilter(IFilter filter)
        {
            lock (Loggers)
            {
                Filters.Add(filter);
            }
        }

        static public string ConvertDirectorySeparatorsFromUnityToOS(string unityFileName)
        {
            return unityFileName.Replace(UnityInternalDirectorySeparator, System.IO.Path.DirectorySeparatorChar);
        }

        static public bool ExtractInfoFromUnityMessage(string log, ref string filename, ref int lineNumber)
        {
            var match = UnityMessageRegex.Matches(log);

            if(match.Count>0)
            {
                filename = match[0].Groups[1].Value;
                lineNumber = Convert.ToInt32(match[0].Groups[2].Value);
                return true;
            }
            return false;
        }
    
        static public bool ExtractInfoFromUnityStackInfo(string log, ref string declaringType, ref string methodName, ref string filename, ref int lineNumber)
        {
            var match = System.Text.RegularExpressions.Regex.Matches(log, @"(.*)\.(.*)\s*\(.*\(at (.*):(\d+)");

            if(match.Count>0)
            {
                declaringType = match[0].Groups[1].Value;
                methodName = match[0].Groups[2].Value;
                filename = match[0].Groups[3].Value;
                lineNumber = Convert.ToInt32(match[0].Groups[4].Value);
                return true;
            }
            return false;
        }

        struct IgnoredUnityMethod
        {
            public enum Mode { Show, ShowIfFirstIgnoredMethod, Hide };
            public string DeclaringTypeName;
            public string MethodName;
            public Mode ShowHideMode;
        }

        static IgnoredUnityMethod[] IgnoredUnityMethods = new IgnoredUnityMethod[]
        {
            new IgnoredUnityMethod { DeclaringTypeName = "Application", MethodName = "CallLogCallback", ShowHideMode = IgnoredUnityMethod.Mode.Hide },
            new IgnoredUnityMethod { DeclaringTypeName = "DebugLogHandler", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.Hide },
            new IgnoredUnityMethod { DeclaringTypeName = "Logger", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod },
            new IgnoredUnityMethod { DeclaringTypeName = "Debug", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod },
            new IgnoredUnityMethod { DeclaringTypeName = "Assert", MethodName = null, ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod  },
        };

        static IgnoredUnityMethod.Mode ShowOrHideMethod(MethodBase method)
        {
            foreach (IgnoredUnityMethod ignoredUnityMethod in IgnoredUnityMethods)
            {
                if ((method.DeclaringType.Name == ignoredUnityMethod.DeclaringTypeName) && ((ignoredUnityMethod.MethodName == null) || (method.Name == ignoredUnityMethod.MethodName)))
                {
                    return ignoredUnityMethod.ShowHideMode;
                }
            }

            return IgnoredUnityMethod.Mode.Show;
        }

        [StackTraceIgnore]
        static bool GetCallstack(ref List<LogStackFrame> callstack, out LogStackFrame originatingSourceLocation)
        {
            callstack.Clear();
            StackTrace stackTrace = new StackTrace(true);      
            StackFrame[] stackFrames = stackTrace.GetFrames(); 

            bool encounteredIgnoredMethodPreviously = false;

            originatingSourceLocation = null;

            for (int i = stackFrames.Length - 1; i >= 0; i--)
            {
                StackFrame stackFrame = stackFrames[i];

                var method = stackFrame.GetMethod();
                if(method.IsDefined(typeof(LogUnityOnly), true))
                {
                    return true;
                }
                if(!method.IsDefined(typeof(StackTraceIgnore), true))
                {
                    IgnoredUnityMethod.Mode showHideMode = ShowOrHideMethod(method);

                    bool setOriginatingSourceLocation = (showHideMode == IgnoredUnityMethod.Mode.Show);

                    if (showHideMode == IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod)
                    {
                        if (!encounteredIgnoredMethodPreviously)
                        {
                            encounteredIgnoredMethodPreviously = true;
                            showHideMode = IgnoredUnityMethod.Mode.Show;
                        }
                        else
                            showHideMode = IgnoredUnityMethod.Mode.Hide;
                    }

                    if (showHideMode == IgnoredUnityMethod.Mode.Show)
                    {
                        var logStackFrame = new LogStackFrame(stackFrame);
                        
                        callstack.Add(logStackFrame);

                        if (setOriginatingSourceLocation)
                            originatingSourceLocation = logStackFrame;
                    }
                }
            }

            callstack.Reverse();
        
            return false;
        }

        static List<LogStackFrame> GetCallstackFromUnityLog(string unityCallstack, out LogStackFrame originatingSourceLocation)
        {
            var lines = System.Text.RegularExpressions.Regex.Split(unityCallstack, UberLogger.Logger.UnityInternalNewLine);

            var stack = new List<LogStackFrame>();
            foreach(var line in lines)
            {
                var frame = new LogStackFrame(line);
                if(!string.IsNullOrEmpty(frame.GetFormattedMethodNameWithFileName()))
                {
                    stack.Add(new LogStackFrame(line));
                }
            }

            if (stack.Count > 0)
                originatingSourceLocation = stack[0];
            else
                originatingSourceLocation = null;

            return stack;
        }

        [StackTraceIgnore()]
        static void UnityLogInternal(string unityMessage, string unityCallStack, UnityEngine.LogType logType)
        {
            lock(Loggers)
            {
                if(!AlreadyLogging)
                {
                    try
                    {
                        AlreadyLogging = true;
                    
                        var callstack = new List<LogStackFrame>();
                        LogStackFrame originatingSourceLocation;
                        var unityOnly = GetCallstack(ref callstack, out originatingSourceLocation);
                        if(unityOnly)
                        {
                            return;
                        }

                        if(callstack.Count==0)
                        {
                            callstack = GetCallstackFromUnityLog(unityCallStack, out originatingSourceLocation);
                        }

                        LogSeverity severity;
                        switch(logType)
                        {
                            case UnityEngine.LogType.Error: severity = LogSeverity.Error; break;
                            case UnityEngine.LogType.Assert: severity = LogSeverity.Error; break;
                            case UnityEngine.LogType.Exception: severity = LogSeverity.Error; break;
                            case UnityEngine.LogType.Warning: severity = LogSeverity.Warning; break;
                            default: severity = LogSeverity.Message; break;
                        }

                        string filename = "";
                        int lineNumber = 0;
                    
                        if(ExtractInfoFromUnityMessage(unityMessage, ref filename, ref lineNumber))
                        {
                            callstack.Insert(0, new LogStackFrame(unityMessage, filename, lineNumber));
                        }

                        var logInfo = new LogInfo(null, "", severity, callstack, originatingSourceLocation, unityMessage);

                        RecentMessages.AddLast(logInfo);

                        TrimOldMessages();

                        Loggers.RemoveAll(l=>l==null);
                        Loggers.ForEach(l=>l.Log(logInfo));
                    }
                    finally
                    {
                        AlreadyLogging = false;
                    }
                }
            }
        }

        [StackTraceIgnore()]
        static public void Log(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
        {
            lock(Loggers)
            {
                if(!AlreadyLogging)
                {
                    try
                    {
                        AlreadyLogging = true;

                        foreach (IFilter filter in Filters)
                        {
                            if (!filter.ApplyFilter(channel, source, severity, message, par))
                                return;
                        }
						
                        var callstack = new List<LogStackFrame>();
                        LogStackFrame originatingSourceLocation;
                        var unityOnly = GetCallstack(ref callstack, out originatingSourceLocation);
                        if(unityOnly)
                        {
                            return;
                        }

                        var logInfo = new LogInfo(source, channel, severity, callstack, originatingSourceLocation, message, par);

                        RecentMessages.AddLast(logInfo);

                        TrimOldMessages();

                        Loggers.RemoveAll(l=>l==null);
                        Loggers.ForEach(l=>l.Log(logInfo));

                        if(ForwardMessages)
                        {
                            ForwardToUnity(source, severity, message, par);
                        }
                    }
                    finally
                    {
                        AlreadyLogging = false;
                    }
                }
            }
        }

        [LogUnityOnly()]
        static void ForwardToUnity(UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
        {
			object showObject = null;
            if(message!=null)
            {
				var messageAsString = message as string;
				if(messageAsString!=null)
				{
	                if(par.Length>0)
    	            {
        	            showObject = String.Format(messageAsString, par);
            	    }
            	    else
               		{
                    	showObject = message;
                	}
				}
				else
				{
					showObject = message;
				}
            }

            if(source==null)
            {
				if(severity==LogSeverity.Message) UnityEngine.Debug.Log(showObject);
                else if(severity==LogSeverity.Warning) UnityEngine.Debug.LogWarning(showObject);
                else if(severity==LogSeverity.Error) UnityEngine.Debug.LogError(showObject);
            }
            else
            {
                if(severity==LogSeverity.Message) UnityEngine.Debug.Log(showObject, source);
                else if(severity==LogSeverity.Warning) UnityEngine.Debug.LogWarning(showObject, source);
                else if(severity==LogSeverity.Error) UnityEngine.Debug.LogError(showObject, source);
            }
        }

        static public T GetLogger<T>() where T:class
        {
            foreach(var logger in Loggers)
            {
                if(logger is T)
                {
                    return logger as T;
                }
            }
            return null;
        }

        static void TrimOldMessages()
        {
            while(RecentMessages.Count > MaxMessagesToKeep)
            {
                RecentMessages.RemoveFirst();
            }
        }
    }

}
