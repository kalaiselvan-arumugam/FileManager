using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Amazon.S3;

namespace S3FileManager;

/// <summary>
/// Main entry point for the S3/MinIO File Management System.
/// Configures logging, dependency injection, and application lifecycle.
/// </summary>
public class Program
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Main entry point - configures services and runs the application.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Exit code</returns>
    public static async Task<int> Main(string[] args)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "S3FileManager.log");

        try
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole()
                    .AddProvider(new FileLoggerProvider(logPath));
            });

            var logger = _loggerFactory.CreateLogger<Program>();

            logger.LogInformation("=================================================");
            logger.LogInformation("S3/MinIO File Management System Starting...");
            logger.LogInformation("=================================================");

            var configurationLoader = new ConfigurationLoader(
                _loggerFactory.CreateLogger<ConfigurationLoader>());

            var config = configurationLoader.Load();

            var s3ClientFactory = new S3ClientFactory(
                _loggerFactory.CreateLogger<S3ClientFactory>(),
                config);

            var s3Client = s3ClientFactory.CreateClient();

            logger.LogInformation("Validating bucket exists: {BucketName}", config.BucketName);
            var bucketExists = await s3ClientFactory.ValidateBucketExistsAsync(s3Client);

            if (!bucketExists)
            {
                logger.LogError("Bucket '{BucketName}' does not exist or is not accessible.", config.BucketName);
                Console.Error.WriteLine($"ERROR: Bucket '{config.BucketName}' does not exist or is not accessible.");
                return 1;
            }

            logger.LogInformation("Bucket validation successful");

            var storageService = new S3StorageService(
                _loggerFactory.CreateLogger<S3StorageService>(),
                s3Client,
                config.BucketName);

            var consoleHelper = new ConsoleHelper(
                _loggerFactory.CreateLogger<ConsoleHelper>());

            var menuService = new MenuService(
                _loggerFactory.CreateLogger<MenuService>(),
                consoleHelper,
                storageService);

            await menuService.RunMenuLoopAsync();

            logger.LogInformation("Application shutdown complete");

            return 0;
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"ERROR: Configuration file not found - {ex.Message}");
            return 1;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"ERROR: Invalid configuration - {ex.Message}");
            return 1;
        }
        catch (AmazonS3Exception ex)
        {
            Console.Error.WriteLine($"ERROR: S3 operation failed - {ex.Message}");
            if (_loggerFactory != null)
            {
                var logger = _loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "S3 error occurred");
            }
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: Unexpected error - {ex.Message}");
            if (_loggerFactory != null)
            {
                var logger = _loggerFactory.CreateLogger<Program>();
                logger.LogCritical(ex, "Unhandled exception occurred");
            }
            return 1;
        }
        finally
        {
            _loggerFactory?.Dispose();
        }
    }
}