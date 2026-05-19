namespace S3FileManager;

/// <summary>
/// Provides path resolution utilities for file operations.
/// Ensures all paths are resolved relative to the executable directory.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Gets the application base directory (where the executable is located).
    /// </summary>
    public static string BaseDirectory => AppContext.BaseDirectory;

    /// <summary>
    /// Gets the full path for a configuration file in the application directory.
    /// </summary>
    /// <param name="fileName">Name of the configuration file</param>
    /// <returns>Full path to the configuration file</returns>
    public static string GetConfigFilePath(string fileName)
    {
        return Path.Combine(BaseDirectory, fileName);
    }

    /// <summary>
    /// Gets the full path for a file relative to the application directory.
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Full path to the file</returns>
    public static string GetFilePath(string fileName)
    {
        return Path.Combine(BaseDirectory, fileName);
    }

    /// <summary>
    /// Gets the full path for a certificate file relative to the application directory.
    /// </summary>
    /// <param name="certificateFileName">Name of the certificate file</param>
    /// <returns>Full path to the certificate file</returns>
    public static string GetCertificatePath(string certificateFileName)
    {
        return Path.Combine(BaseDirectory, certificateFileName);
    }

    /// <summary>
    /// Checks if a file exists in the application directory.
    /// </summary>
    /// <param name="fileName">Name of the file to check</param>
    /// <returns>True if file exists, false otherwise</returns>
    public static bool FileExists(string fileName)
    {
        var fullPath = GetFilePath(fileName);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Normalizes path separators for the current platform.
    /// </summary>
    /// <param name="path">Path to normalize</param>
    /// <returns>Normalized path</returns>
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Ensures path ends with forward slash for S3 prefix operations.
    /// </summary>
    /// <param name="prefix">Prefix to ensure ends with slash</param>
    /// <returns>Prefix ending with slash</returns>
    public static string EnsureTrailingSlash(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return string.Empty;

        return prefix.TrimEnd('/') + "/";
    }
}