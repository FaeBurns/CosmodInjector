using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace CosmoteerModLib;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Logger
{
    private static readonly HashSet<string> s_writtenLogNames = new HashSet<string>();
    private static int s_logIndex;
    private static DirectoryInfo s_logFolder = null!;
    private static readonly List<Logger> s_loggers = new List<Logger>();
    private static readonly Dictionary<string, Logger> s_lookup = new Dictionary<string, Logger>();

    private readonly Queue<Message> m_messageQueue = new Queue<Message>();

    internal string LogName { get; }

    internal static void Init(DirectoryInfo gameDir, int logIndex)
    {
        s_logFolder = new DirectoryInfo(Path.Combine(gameDir.FullName, "Logs"));
        s_logIndex = logIndex;
        Directory.CreateDirectory(s_logFolder.FullName);
    }

    internal Logger(string logName)
    {
        LogName = logName;
    }

    /// <summary>
    /// Gets a logger with the specified name.
    /// </summary>
    /// <param name="logName">The name of the log.</param>
    /// <returns>The logger instance.</returns>
    public static Logger GetLogger(string logName)
    {
        if (s_lookup.TryGetValue(logName, out Logger? logger))
        {
            return logger;
        }

        Logger newLogger = new Logger(logName);
        s_loggers.Add(newLogger);
        s_lookup.Add(logName, newLogger);

        return newLogger;
    }

    /// <summary>
    /// Gets the logger for the calling assembly.
    /// <seealso cref="Assembly.GetCallingAssembly"/>
    /// </summary>
    /// <returns></returns>
    public static Logger GetLocal()
    {
        return GetLogger(Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location));
    }

    /// <summary>
    /// Logs to the <see cref="Logger"/> instance retrieved from <see cref="GetLocal"/>.
    /// </summary>
    public static void LogLocal(string message)
    {
        // cannot use GetLocal as that will use GetCallingAssembly on this method and get CosmoteerModLib instead of intended assembly
        GetLogger(Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location)).Log(message);
    }

    internal static void FlushAll(LogFlushMode flushMode)
    {
        switch (flushMode)
        {
            case LogFlushMode.SINGLE_FILE:
                using (StreamWriter logStream = GetLogStreamFromName("log"))
                {
                    foreach (Logger logger in s_loggers)
                    {
                        logger.FlushToStream(logStream, true);
                    }
                }
                break;
            case LogFlushMode.MULTIPLE_FILES:
                foreach (Logger logger in s_loggers)
                {
                    using StreamWriter logStream = GetLogStreamFromName(logger.LogName);
                    logger.FlushToStream(logStream, false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(flushMode), flushMode, null);
        }
    }

    private static StreamWriter GetLogStreamFromName(string name)
    {
        string logPath = GetLogPath(name);

        if (!s_writtenLogNames.Contains(name))
        {
            File.WriteAllText(logPath, String.Empty);
            s_writtenLogNames.Add(name);
        }

        return new StreamWriter(logPath, true);
    }

    private static string GetLogPath(string name)
    {
        return Path.Combine(s_logFolder.FullName, name + (s_logIndex > 0 ? s_logIndex.ToString() : "") + ".txt");
    }

    public void Log(string message)
    {
        m_messageQueue.Enqueue(new Message(message));
    }

    private void FlushToStream(StreamWriter stream, bool includeName)
    {
        StringBuilder stringBuilder = new StringBuilder();
        while (m_messageQueue.Count > 0)
        {
            Message message = m_messageQueue.Dequeue();

            stringBuilder.Append("[");
            stringBuilder.Append(message.LogTime.ToString(Message.COMBINED_FORMAT_STRING));
            stringBuilder.Append("] ");

            if (includeName)
                stringBuilder.AppendFormat("[{0}] ", LogName);

            stringBuilder.AppendLine(message.Data);
        }
        stream.Write(stringBuilder);
    }

    internal enum LogFlushMode
    {
        SINGLE_FILE,
        MULTIPLE_FILES,
    }

    private readonly struct Message
    {
        private const string DATE_FORMAT_STRING = "dd/MM/yy";
        private const string TIME_FORMAT_STRING = "HH:mm:ss";
        public const string COMBINED_FORMAT_STRING = DATE_FORMAT_STRING + " " + TIME_FORMAT_STRING;

        public readonly DateTime LogTime;
        public readonly string Data;

        public Message(string data)
        {
            Data = data;
            LogTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{LogTime.ToString(COMBINED_FORMAT_STRING)}] {Data}";
        }
    }
}