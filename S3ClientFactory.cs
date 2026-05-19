using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace S3FileManager;

/// <summary>
/// Factory for creating S3 client with proper configuration for MinIO, AWS S3, and S3-compatible storage.
/// Supports SSL/TLS, mTLS, and custom endpoints.
/// </summary>
public class S3ClientFactory
{
    private readonly ILogger<S3ClientFactory> _logger;
    private readonly AppConfiguration _configuration;

    public S3ClientFactory(ILogger<S3ClientFactory> logger, AppConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Creates and configures an S3 client based on the application configuration.
    /// </summary>
    /// <returns>Configured AmazonS3Client instance</returns>
    public AmazonS3Client CreateClient()
    {
        _logger.LogInformation("Creating S3 client for endpoint: {Endpoint}", _configuration.Endpoint);
        _logger.LogInformation("UseSSL: {UseSSL}, Timeout: {Timeout}s",
            _configuration.UseSSL, _configuration.ConnectionTimeoutSeconds);

        var config = new AmazonS3Config
        {
            ServiceURL = _configuration.Endpoint,
            UseHttp = !_configuration.UseSSL,
            ForcePathStyle = _configuration.PathStyleAccess,
            Timeout = TimeSpan.FromSeconds(_configuration.ConnectionTimeoutSeconds),
            MaxErrorRetry = 3
        };

        if (_configuration.DisableCertificateValidation)
        {
            _logger.LogWarning("DEV ONLY: Certificate validation is DISABLED. This is insecure and should only be used in development.");
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        X509Certificate2? clientCertificate = null;
        if (!string.IsNullOrWhiteSpace(_configuration.P12Path))
        {
            var certPath = PathHelper.GetCertificatePath(_configuration.P12Path);
            _logger.LogInformation("Loading client certificate from: {CertPath}", certPath);

            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException($"Client certificate not found: {certPath}");
            }

            try
            {
                clientCertificate = new X509Certificate2(
                    certPath,
                    _configuration.P12Password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                _logger.LogInformation("Client certificate loaded successfully. Subject: {Subject}", clientCertificate.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load client certificate");
                throw new InvalidOperationException($"Failed to load client certificate: {ex.Message}", ex);
            }
        }

        var credentials = new Amazon.Runtime.AnonymousAWSCredentials();

        if (clientCertificate != null)
        {
            var handler = CreateHttpClientHandlerWithClientCertificate(clientCertificate);
            var httpClient = new HttpClient(handler);
            return new AmazonS3Client(credentials, config);
        }

        return new AmazonS3Client(credentials, config);
    }

    /// <summary>
    /// Creates HttpClientHandler with client certificate for mTLS.
    /// </summary>
    private HttpClientHandler CreateHttpClientHandlerWithClientCertificate(X509Certificate2 clientCertificate)
    {
        var handler = new HttpClientHandler();

        if (_configuration.DisableCertificateValidation)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ClientCertificates.Add(clientCertificate);

        return handler;
    }

    /// <summary>
    /// Validates that the configured bucket exists.
    /// </summary>
    /// <param name="client">The S3 client to use for validation</param>
    /// <returns>True if bucket exists, false otherwise</returns>
    public async Task<bool> ValidateBucketExistsAsync(AmazonS3Client client)
    {
        try
        {
            _logger.LogInformation("Validating bucket exists: {BucketName}", _configuration.BucketName);

            var request = new ListObjectsV2Request
            {
                BucketName = _configuration.BucketName,
                MaxKeys = 1
            };

            var response = await client.ListObjectsV2Async(request);

            _logger.LogInformation("Bucket '{BucketName}' exists and is accessible", _configuration.BucketName);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound ||
                ex.ErrorCode == "NoSuchBucket")
            {
                _logger.LogError("Bucket '{BucketName}' does not exist", _configuration.BucketName);
                return false;
            }

            _logger.LogError(ex, "Error validating bucket: {BucketName}", _configuration.BucketName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating bucket: {BucketName}", _configuration.BucketName);
            throw;
        }
    }
}