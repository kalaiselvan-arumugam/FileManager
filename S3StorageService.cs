using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace S3FileManager;

/// <summary>
/// Provides S3 storage operations including upload, copy, delete, and listing.
/// Uses server-side S3 copy operations for efficiency.
/// </summary>
public class S3StorageService
{
    private readonly ILogger<S3StorageService> _logger;
    private readonly AmazonS3Client _client;
    private readonly string _bucketName;

    public S3StorageService(ILogger<S3StorageService> logger, AmazonS3Client client, string bucketName)
    {
        _logger = logger;
        _client = client;
        _bucketName = bucketName;
    }

    /// <summary>
    /// Creates a folder (empty object with trailing slash prefix) in S3.
    /// </summary>
    /// <param name="prefix">S3 prefix representing the folder</param>
    public async Task CreateFolderAsync(string prefix)
    {
        try
        {
            var normalizedPrefix = PathHelper.EnsureTrailingSlash(prefix);
            _logger.LogInformation("Creating folder: {FolderPrefix}", normalizedPrefix);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = normalizedPrefix,
                ContentBody = string.Empty,
                ContentType = "application/x-directory"
            };

            var response = await _client.PutObjectAsync(request);

            _logger.LogInformation("Folder created successfully: {FolderPrefix}, ETag: {ETag}",
                normalizedPrefix, response.ETag);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error creating folder: {FolderPrefix}", prefix);
            throw new InvalidOperationException($"Failed to create folder '{prefix}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder: {FolderPrefix}", prefix);
            throw;
        }
    }

    /// <summary>
    /// Uploads a file from local filesystem to S3.
    /// </summary>
    /// <param name="localFilePath">Full path to local file</param>
    /// <param name="s3Key">S3 object key (including prefix)</param>
    public async Task UploadFileAsync(string localFilePath, string s3Key)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                throw new FileNotFoundException($"Local file not found: {localFilePath}");
            }

            _logger.LogInformation("Uploading file: {LocalFile} to s3://{BucketName}/{S3Key}",
                localFilePath, _bucketName, s3Key);

            var fileInfo = new FileInfo(localFilePath);

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                InputStream = fileStream,
                ContentType = "text/plain",
                AutoCloseStream = true
            };

            var response = await _client.PutObjectAsync(request);

            _logger.LogInformation("Upload completed. File: {S3Key}, Size: {FileSize} bytes, ETag: {ETag}",
                s3Key, fileInfo.Length, response.ETag);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found: {LocalFile}", localFilePath);
            throw;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error uploading file: {LocalFile}", localFilePath);
            throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {LocalFile}", localFilePath);
            throw;
        }
    }

    /// <summary>
    /// Copies an S3 object to another location using server-side copy.
    /// Does NOT download and re-upload - uses efficient S3 copy operation.
    /// </summary>
    /// <param name="sourceKey">Source object key</param>
    /// <param name="destinationKey">Destination object key</param>
    public async Task CopyObjectAsync(string sourceKey, string destinationKey)
    {
        try
        {
            _logger.LogInformation("Copying s3://{BucketName}/{SourceKey} to s3://{BucketName2}/{DestKey}",
                _bucketName, _bucketName, sourceKey, destinationKey);

            var request = new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                DestinationBucket = _bucketName,
                SourceKey = sourceKey,
                DestinationKey = destinationKey
            };

            var response = await _client.CopyObjectAsync(request);

            _logger.LogInformation("Copy completed. Destination: {DestKey}, VersionId: {VersionId}",
                destinationKey, response.VersionId);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error copying object: {SourceKey} to {DestKey}", sourceKey, destinationKey);
            throw new InvalidOperationException($"Failed to copy object: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying object: {SourceKey} to {DestKey}", sourceKey, destinationKey);
            throw;
        }
    }

    /// <summary>
    /// Moves an S3 object by copying to destination and then deleting source.
    /// </summary>
    /// <param name="sourceKey">Source object key</param>
    /// <param name="destinationKey">Destination object key</param>
    public async Task MoveObjectAsync(string sourceKey, string destinationKey)
    {
        try
        {
            _logger.LogInformation("Moving s3://{BucketName}/{SourceKey} to s3://{BucketName2}/{DestKey}",
                _bucketName, _bucketName, sourceKey, destinationKey);

            await CopyObjectAsync(sourceKey, destinationKey);

            await DeleteObjectAsync(sourceKey);

            _logger.LogInformation("Move completed successfully: {SourceKey} -> {DestKey}", sourceKey, destinationKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving object: {SourceKey} to {DestKey}", sourceKey, destinationKey);
            throw;
        }
    }

    /// <summary>
    /// Deletes an object from S3.
    /// </summary>
    /// <param name="key">Object key to delete</param>
    public async Task DeleteObjectAsync(string key)
    {
        try
        {
            _logger.LogInformation("Deleting s3://{BucketName}/{Key}", _bucketName, key);

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _client.DeleteObjectAsync(request);

            _logger.LogInformation("Object deleted successfully: {Key}", key);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error deleting object: {Key}", key);
            throw new InvalidOperationException($"Failed to delete object: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting object: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Lists all objects and folders recursively in the bucket.
    /// </summary>
    /// <returns>List of S3 object summaries</returns>
    public async Task<List<S3Object>> ListObjectsRecursivelyAsync()
    {
        var allObjects = new List<S3Object>();

        try
        {
            _logger.LogInformation("Listing all objects recursively in bucket: {BucketName}", _bucketName);

            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName
            };

            ListObjectsV2Response response;

            do
            {
                response = await _client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {
                    allObjects.Add(obj);
                    _logger.LogDebug("Found object: {Key}, Size: {Size}", obj.Key, obj.Size);
                }

                request.ContinuationToken = response.NextContinuationToken;

            } while (response.IsTruncated);

            _logger.LogInformation("Listed {ObjectCount} objects recursively", allObjects.Count);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error listing objects");
            throw new InvalidOperationException($"Failed to list objects: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing objects");
            throw;
        }

        return allObjects;
    }

    /// <summary>
    /// Lists all objects with a specific prefix.
    /// </summary>
    /// <param name="prefix">S3 prefix to filter objects</param>
    /// <returns>List of S3 object summaries</returns>
    public async Task<List<S3Object>> ListObjectsWithPrefixAsync(string prefix)
    {
        var allObjects = new List<S3Object>();

        try
        {
            _logger.LogInformation("Listing objects with prefix: {Prefix} in bucket: {BucketName}", prefix, _bucketName);

            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            ListObjectsV2Response response;

            do
            {
                response = await _client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {
                    allObjects.Add(obj);
                    _logger.LogDebug("Found object: {Key}, Size: {Size}", obj.Key, obj.Size);
                }

                request.ContinuationToken = response.NextContinuationToken;

            } while (response.IsTruncated);

            _logger.LogInformation("Listed {ObjectCount} objects with prefix: {Prefix}", allObjects.Count, prefix);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error listing objects with prefix: {Prefix}", prefix);
            throw new InvalidOperationException($"Failed to list objects with prefix '{prefix}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing objects with prefix: {Prefix}", prefix);
            throw;
        }

        return allObjects;
    }

    /// <summary>
    /// Checks if an object exists in S3.
    /// </summary>
    /// <param name="key">Object key to check</param>
    /// <returns>True if object exists</returns>
    public async Task<bool> ObjectExistsAsync(string key)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <summary>
    /// Downloads all files from S3 bucket to a local folder.
    /// </summary>
    /// <param name="localDownloadFolder">Local folder path to save downloaded files</param>
    /// <param name="prefix">S3 prefix to filter downloads (optional, defaults to Bas-887766/)</param>
    /// <returns>Number of files downloaded</returns>
    public async Task<int> DownloadAllFilesAsync(string localDownloadFolder, string? prefix = null)
    {
        prefix ??= "Bas-887766/";

        try
        {
            _logger.LogInformation("Starting download of all files from prefix: {Prefix} to: {DownloadFolder}", prefix, localDownloadFolder);

            if (!Directory.Exists(localDownloadFolder))
            {
                Directory.CreateDirectory(localDownloadFolder);
                _logger.LogInformation("Created download directory: {DownloadFolder}", localDownloadFolder);
            }

            var objects = await ListObjectsWithPrefixAsync(prefix);
            var filesOnly = objects.Where(o => !o.Key.EndsWith("/")).ToList();

            if (filesOnly.Count == 0)
            {
                _logger.LogInformation("No files found to download");
                return 0;
            }

            var downloadedCount = 0;

            foreach (var obj in filesOnly)
            {
                var localFilePath = Path.Combine(localDownloadFolder, obj.Key);

                var directory = Path.GetDirectoryName(localFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logger.LogInformation("Downloading: s3://{Bucket}/{Key} to {LocalPath}", _bucketName, obj.Key, localFilePath);

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = obj.Key
                };

                using var response = await _client.GetObjectAsync(request);
                await response.WriteResponseStreamToFileAsync(localFilePath, true, default);

                downloadedCount++;
                _logger.LogInformation("Downloaded: {Key} ({Size} bytes)", obj.Key, obj.Size);
            }

            _logger.LogInformation("Download completed. Total files downloaded: {Count}", downloadedCount);
            return downloadedCount;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error downloading files");
            throw new InvalidOperationException($"Failed to download files: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading files");
            throw;
        }
    }
}