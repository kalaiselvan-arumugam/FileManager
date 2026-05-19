using Microsoft.Extensions.Logging;

namespace S3FileManager;

/// <summary>
/// Helper class for console input/output operations.
/// </summary>
public class ConsoleHelper
{
    private readonly ILogger<ConsoleHelper> _logger;

    public ConsoleHelper(ILogger<ConsoleHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Writes a line to console with optional color.
    /// </summary>
    public void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Writes a formatted line to console.
    /// </summary>
    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }

    /// <summary>
    /// Writes a message to console without newline.
    /// </summary>
    public void Write(string message)
    {
        Console.Write(message);
    }

    /// <summary>
    /// Reads a line from console.
    /// </summary>
    public string ReadLine()
    {
        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Reads a key from console without waiting for Enter.
    /// </summary>
    public ConsoleKeyInfo ReadKey(bool intercept = true)
    {
        return Console.ReadKey(intercept);
    }

    /// <summary>
    /// Clears the console screen.
    /// </summary>
    public void Clear()
    {
        Console.Clear();
    }

    /// <summary>
    /// Displays a formatted error message to the console.
    /// </summary>
    public void DisplayError(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {message}");
        Console.ForegroundColor = originalColor;
        _logger.LogError("{Message}", message);
    }

    /// <summary>
    /// Displays a formatted success message to the console.
    /// </summary>
    public void DisplaySuccess(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"SUCCESS: {message}");
        Console.ForegroundColor = originalColor;
        _logger.LogInformation("{Message}", message);
    }

    /// <summary>
    /// Displays a formatted warning message to the console.
    /// </summary>
    public void DisplayWarning(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"WARNING: {message}");
        Console.ForegroundColor = originalColor;
        _logger.LogWarning("{Message}", message);
    }

    /// <summary>
    /// Displays a formatted info message to the console.
    /// </summary>
    public void DisplayInfo(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"INFO: {message}");
        Console.ForegroundColor = originalColor;
        _logger.LogInformation("{Message}", message);
    }

    /// <summary>
    /// Prompts user and waits for Enter key to continue.
    /// </summary>
    public void WaitForContinue()
    {
        WriteLine();
        WriteLine(Constants.ContinuePrompt);
        ReadLine();
    }

    /// <summary>
    /// Gets user input with a prompt displayed.
    /// </summary>
    public string GetInput(string prompt)
    {
        Write(prompt);
        return ReadLine();
    }
}