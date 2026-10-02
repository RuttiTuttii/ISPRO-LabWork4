namespace LabWork4.Logging;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public interface IAppLogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}

public class ConsoleAppLogger : IAppLogger
{
    private readonly string _category;

    public ConsoleAppLogger(string category)
    {
        _category = category;
    }

    public void LogInfo(string message)
    {
        WriteLog(LogLevel.Info, ConsoleColor.Cyan, message);
    }

    public void LogWarning(string message)
    {
        WriteLog(LogLevel.Warning, ConsoleColor.Yellow, message);
    }

    public void LogError(string message, Exception? ex = null)
    {
        var fullMessage = ex == null ? message : $"{message} | [ошибка: {ex.GetType().Name}: {ex.Message}]";
        WriteLog(LogLevel.Error, ConsoleColor.Red, fullMessage);
    }

    private void WriteLog(LogLevel level, ConsoleColor color, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{timestamp}] [{level.ToString().ToLowerInvariant(),-7}] [{_category}] {message}");
        Console.ForegroundColor = originalColor;
    }
}
