namespace CosmoteerModInjector;

public static class Logger
{
    private const string LogFile = "CMI_log.txt";

    public static void Init()
    {
        using StreamWriter sw = File.CreateText(LogFile);
        sw.Write("");
    }

    public static void Log(string message)
    {
        string logLine = "[ " + DateTime.Now.ToString("MM/dd/yy hh:mm:ss") + "] " + message + Environment.NewLine;
        File.AppendAllText(LogFile, logLine);
    }
}