using Microsoft.Extensions.Logging;

namespace S3FileManager;

/// <summary>
/// Loads configuration from external .properties file.
/// </summary>
public class ConfigurationLoader
{
    private readonly ILogger<ConfigurationLoader> _logger;
    private const string ConfigFileName = "config.properties";

    public ConfigurationLoader(ILogger<ConfigurationLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads configuration from config.properties file located in the application base directory.
    /// </summary>
    /// <returns>Populated AppConfiguration instance</returns>
    /// <exception cref="FileNotFoundException">Thrown when config file does not exist</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public AppConfiguration Load()
    {
        var configPath = PathHelper.GetConfigFilePath(ConfigFileName);

        if (!File.Exists(configPath))
        {
            _logger.LogError("Configuration file not found: {ConfigPath}", configPath);
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        _logger.LogInformation("Loading configuration from: {ConfigPath}", configPath);

        var properties = ParsePropertiesFile(configPath);
        var config = MapToConfiguration(properties);

        config.Validate();

        _logger.LogInformation("Configuration loaded successfully");
        _logger.LogInformation("Endpoint: {Endpoint}, Bucket: {Bucket}, UseSSL: {UseSSL}",
            config.Endpoint, config.BucketName, config.UseSSL);

        return config;
    }

    /// <summary>
    /// Manually parses .properties file without external libraries.
    /// Ignores comments (lines starting with # or !) and blank lines.
    /// </summary>
    private Dictionary<string, string> ParsePropertiesFile(string filePath)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            if (trimmedLine.StartsWith("#") || trimmedLine.StartsWith("!"))
                continue;

            var equalsIndex = trimmedLine.IndexOf('=');
            if (equalsIndex <= 0)
            {
                _logger.LogWarning("Skipping invalid property line: {Line}", trimmedLine);
                continue;
            }

            var key = trimmedLine.Substring(0, equalsIndex).Trim();
            var value = trimmedLine.Substring(equalsIndex + 1).Trim();

            value = UnescapePropertyValue(value);

            properties[key] = value;
            _logger.LogDebug("Parsed property: {Key} = {Value}", key, value);
        }

        return properties;
    }

    /// <summary>
    /// Handles basic property value unescaping (handles \n, \r, \t, \\).
    /// </summary>
    private string UnescapePropertyValue(string value)
    {
        var result = value
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\\", "\\");

        return result;
    }

    /// <summary>
    /// Maps parsed properties to AppConfiguration model.
    /// </summary>
    private AppConfiguration MapToConfiguration(Dictionary<string, string> properties)
    {
        var config = new AppConfiguration();

        if (properties.TryGetValue("endpoint", out var endpoint))
            config.Endpoint = endpoint;

        if (properties.TryGetValue("accessKey", out var accessKey))
            config.AccessKey = accessKey;

        if (properties.TryGetValue("secretKey", out var secretKey))
            config.SecretKey = secretKey;

        if (properties.TryGetValue("region", out var region))
            config.Region = region;

        if (properties.TryGetValue("bucketName", out var bucketName))
            config.BucketName = bucketName;

        if (properties.TryGetValue("p12Path", out var p12Path))
            config.P12Path = p12Path;

        if (properties.TryGetValue("p12Password", out var p12Password))
            config.P12Password = p12Password;

        if (properties.TryGetValue("useSSL", out var useSSL))
            config.UseSSL = ParseBoolean(useSSL, "useSSL", defaultValue: true);

        if (properties.TryGetValue("pathStyleAccess", out var pathStyleAccess))
            config.PathStyleAccess = ParseBoolean(pathStyleAccess, "pathStyleAccess", defaultValue: true);

        if (properties.TryGetValue("disableCertificateValidation", out var disableCertValidation))
            config.DisableCertificateValidation = ParseBoolean(disableCertValidation, "disableCertificateValidation", defaultValue: false);

        if (properties.TryGetValue("connectionTimeoutSeconds", out var timeout))
            config.ConnectionTimeoutSeconds = ParseInt(timeout, "connectionTimeoutSeconds", defaultValue: 60);

        return config;
    }

    private static bool ParseBoolean(string value, string key, bool defaultValue)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed switch
        {
            "true" => true,
            "false" => false,
            "yes" => true,
            "no" => false,
            "1" => true,
            "0" => false,
            _ => defaultValue
        };
    }

    private static int ParseInt(string value, string key, int defaultValue)
    {
        if (int.TryParse(value.Trim(), out var result))
            return result;

        return defaultValue;
    }
}