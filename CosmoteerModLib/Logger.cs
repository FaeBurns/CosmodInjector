using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace CosmoteerModLib;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Logger
{
    private static int _logIndex;
    private static DirectoryInfo _logFolder = null!;
    private static readonly List<Logger> _loggers = new List<Logger>();
    private static readonly Dictionary<string, Logger> _lookup = new Dictionary<string, Logger>();

    private readonly Queue<Message> _messageQueue = new Queue<Message>();

    internal string LogName { get; }

    internal static void Init(DirectoryInfo gameDir, int logIndex)
    {
        _logFolder = new DirectoryInfo(Path.Combine(gameDir.FullName, "Logs"));
        _logIndex = logIndex;
        Directory.CreateDirectory(_logFolder.FullName);
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
        if (_lookup.TryGetValue(logName, out Logger? logger))
        {
            return logger;
        }

        Logger newLogger = new Logger(logName);
        _loggers.Add(newLogger);
        _lookup.Add(logName, newLogger);

        // reset the file
        File.WriteAllText(GetLogPath(logName), String.Empty);

        return newLogger;
    }

    /// <summary>
    /// Gets the logger for the calling assembly.
    /// <seealso cref="Assembly.GetCallingAssembly"/>.
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
            case LogFlushMode.SingleFile:
                using (StreamWriter logStream = GetLogStreamFromName("log"))
                {
                    foreach (Logger logger in _loggers)
                    {
                        logger.FlushToStream(logStream, true);
                    }
                }
                break;
            case LogFlushMode.MultipleFiles:
                foreach (Logger logger in _loggers)
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
        return new StreamWriter(logPath, true);
    }

    private static string GetLogPath(string name)
    {
        return Path.Combine(_logFolder.FullName, name + (_logIndex > 0 ? _logIndex.ToString() : "") + ".txt");
    }

    public void Log(string message)
    {
        _messageQueue.Enqueue(new Message(message));
    }

    private void FlushToStream(StreamWriter stream, bool includeName)
    {
        StringBuilder stringBuilder = new StringBuilder();
        while (_messageQueue.Count > 0)
        {
            Message message = _messageQueue.Dequeue();

            if (includeName)
                stringBuilder.AppendFormat($"[{0}] ", LogName);

            message.Compile(stringBuilder);
            stringBuilder.AppendLine();
        }
        stream.Write(stringBuilder);
    }

    internal enum LogFlushMode
    {
        SingleFile,
        MultipleFiles,
    }

    private readonly struct Message
    {
        private const string DateFormatString = "yy/MM/dd";
        private const string TimeFormatString = "hh:mm:ss";
        private const string CombinedFormatString = DateFormatString + "/" + TimeFormatString;

        private readonly DateTime _logTime;
        public readonly string Data;

        public Message(string data)
        {
            Data = data;
            _logTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{_logTime.ToString(CombinedFormatString)}] {Data}";
        }

        public void Compile(StringBuilder builder)
        {
            builder.Append("[");
            builder.Append(_logTime.ToString(CombinedFormatString));
            builder.Append("] ");
            builder.Append(Data);
        }
    }
}