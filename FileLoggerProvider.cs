using Microsoft.Extensions.Logging;

namespace S3FileManager;

/// <summary>
/// File logger provider that writes logs to a file in the application directory.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public FileLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_logFilePath, categoryName, _lock);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// File logger that writes log entries to a file.
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly string _categoryName;
    private readonly object _lock;

    public FileLogger(string logFilePath, string categoryName, object lockObj)
    {
        _logFilePath = logFilePath;
        _categoryName = categoryName;
        _lock = lockObj;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var level = logLevel switch
        {
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "UNKNOWN"
        };

        var message = formatter(state, exception);
        var logEntry = $"[{timestamp}] [{level}] {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        logEntry += Environment.NewLine;

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch
            {
                // Silently fail if unable to write to log file
            }
        }
    }
}