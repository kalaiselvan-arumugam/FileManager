namespace S3FileManager;

/// <summary>
/// Application configuration model for S3/MinIO connection settings.
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// S3 endpoint URL (e.g., https://localhost:9000)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// AWS access key for authentication
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS secret key for authentication
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS region (default: us-east-1)
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// S3 bucket name for file operations
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Path to client certificate for mTLS (.p12 or .pfx)
    /// </summary>
    public string? P12Path { get; set; }

    /// <summary>
    /// Password for client certificate
    /// </summary>
    public string? P12Password { get; set; }

    /// <summary>
    /// Enable SSL/TLS (default: true)
    /// </summary>
    public bool UseSSL { get; set; } = true;

    /// <summary>
    /// Use path-style access (required for MinIO)
    /// </summary>
    public bool PathStyleAccess { get; set; } = true;

    /// <summary>
    /// Disable certificate validation (DEV ONLY - use with caution)
    /// </summary>
    public bool DisableCertificateValidation { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds (default: 60)
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Validates the configuration and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new ArgumentException("Endpoint is required in configuration.");

        if (string.IsNullOrWhiteSpace(AccessKey))
            throw new ArgumentException("AccessKey is required in configuration.");

        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new ArgumentException("SecretKey is required in configuration.");

        if (string.IsNullOrWhiteSpace(BucketName))
            throw new ArgumentException("BucketName is required in configuration.");

        if (ConnectionTimeoutSeconds <= 0)
            throw new ArgumentException("ConnectionTimeoutSeconds must be greater than zero.");
    }
}