# S3/MinIO File Management System

A production-grade enterprise C# .NET console application for managing folders and files in S3-compatible object storage (AWS S3, MinIO, Ceph RGW, LocalStack).

## Features

- **S3-Compatible Storage Support**: Works with AWS S3, MinIO, Ceph RGW, LocalStack
- **Menu-Driven CLI**: Interactive console interface for file operations
- **Framework Support**: .NET 6, .NET 7, .NET 8
- **Deployment Options**: Framework-dependent and self-contained single-file executable
- **SSL/TLS Support**: HTTPS, TLS 1.2, TLS 1.3, mutual TLS (mTLS)
- **Enterprise Logging**: Timestamp-based logging with Microsoft.Extensions.Logging

## Architecture

```
S3FileManager/
├── S3FileManager.csproj      # Project file with multi-target framework support
├── Program.cs                 # Application entry point
├── AppConfiguration.cs        # Configuration model
├── ConfigurationLoader.cs    # .properties file parser
├── S3ClientFactory.cs        # S3 client factory with SSL/mTLS support
├── S3StorageService.cs       # S3 operations (upload, copy, delete, list)
├── MenuService.cs            # Menu-driven operations
├── ConsoleHelper.cs          # Console I/O helper
├── Constants.cs              # Application constants
├── PathHelper.cs             # Path resolution utilities
├── config.properties         # Example configuration file
├── File_1.txt                # Sample file for upload
├── File_2.txt                # Sample file for upload
└── README.md                 # This file
```

## Prerequisites

- .NET 6.0 SDK or later
- Access to an S3-compatible endpoint (MinIO, AWS S3, LocalStack, Ceph RGW)

## Configuration

Create a `config.properties` file in the executable directory with the following settings:

```properties
# Endpoint URL
endpoint=https://localhost:9000

# Authentication
accessKey=minioadmin
secretKey=minioadmin

# Region
region=us-east-1

# Bucket name
bucketName=test-bucket

# SSL configuration
useSSL=true
pathStyleAccess=true

# Optional: mTLS client certificate
# p12Path=client-cert.p12
# p12Password=changeit

# DEV ONLY: Disable certificate validation
# disableCertificateValidation=false

# Connection timeout
connectionTimeoutSeconds=60
```

### Configuration Properties

| Property | Required | Description |
|----------|----------|-------------|
| `endpoint` | Yes | S3 endpoint URL (https://localhost:9000) |
| `accessKey` | Yes | AWS access key |
| `secretKey` | Yes | AWS secret key |
| `region` | No | AWS region (default: us-east-1) |
| `bucketName` | Yes | Target bucket for operations |
| `p12Path` | No | Path to client certificate for mTLS |
| `p12Password` | No | Password for client certificate |
| `useSSL` | No | Enable SSL/TLS (default: true) |
| `pathStyleAccess` | No | Use path-style access (default: true) |
| `disableCertificateValidation` | No | Disable cert validation for DEV (default: false) |
| `connectionTimeoutSeconds` | No | Connection timeout (default: 60) |

## Building

### Restore Dependencies

```bash
cd S3FileManager
dotnet restore
```

### Build

```bash
dotnet build -c Release
```

## Publishing

### Framework-Dependent Deployment

```bash
dotnet publish -c Release
```

Output: `bin/Release/net8.0/publish/S3FileManager.dll`

### Self-Contained Single Executable (Windows)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

Output: `bin/Release/net8.0/win-x64/publish/S3FileManager.exe`

### Self-Contained Single Executable (Linux)

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

Output: `bin/Release/net8.0/linux-x64/publish/S3FileManager`

### Self-Contained Single Executable (macOS)

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

Output: `bin/Release/net8.0/osx-x64/publish/S3FileManager`

## Running

1. Ensure `config.properties` is in the same directory as the executable
2. Place any upload files (e.g., File_1.txt, File_2.txt) in the executable directory
3. Run the application:

```bash
./S3FileManager
```

## Menu Operations

```
=================================
S3 / MinIO File Management System
=================================

1. Create Master Folder - Bas-887766
2. Create Sub Folder 1
3. Create Sub Folder 2
4. Upload File_1.txt to Sub Folder 1
5. Upload File_2.txt to Sub Folder 2
6. Copy File_1.txt to Sub Folder 2
7. Move File_2.txt to Sub Folder 1
8. Delete File_1.txt from Sub Folder 1
9. List All Files and Folders Recursively
0. Exit
```

### Operations Description

1. **Create Master Folder**: Creates `Bas-887766/` prefix
2. **Create Sub Folder 1**: Creates `Bas-887766/sub_folder_1/` prefix
3. **Create Sub Folder 2**: Creates `Bas-887766/sub_folder_2/` prefix
4. **Upload File_1.txt**: Uploads local File_1.txt to sub_folder_1
5. **Upload File_2.txt**: Uploads local File_2.txt to sub_folder_2
6. **Copy File_1.txt**: Server-side copy to sub_folder_2
7. **Move File_2.txt**: Copy then delete (server-side)
8. **Delete File_1.txt**: Removes from sub_folder_1
9. **List All**: Recursively lists all objects

## SSL/mTLS Setup

### For HTTPS with Standard CA Certificates

No additional configuration needed if using certificates signed by a trusted CA.

### For Self-Signed Certificates (Development Only)

```properties
useSSL=true
disableCertificateValidation=true
```

WARNING: Only use `disableCertificateValidation=true` in development. Never use in production.

### For Mutual TLS (mTLS)

1. Place your client certificate (.p12 or .pfx) in the executable directory
2. Configure in config.properties:

```properties
p12Path=client-cert.p12
p12Password=your-certificate-password
useSSL=true
```

## Sample Terminal Output

```
=================================================
S3/MinIO File Management System Starting...
=================================================
2026-05-19 10:15:30 [INFO ] Loading configuration from: C:\path\to\S3FileManager\config.properties
2026-05-19 10:15:30 [INFO ] Configuration loaded successfully
2026-05-19 10:15:30 [INFO ] Creating S3 client for endpoint: https://localhost:9000
2026-05-19 10:15:31 [INFO ] Validating bucket exists: test-bucket
2026-05-19 10:15:31 [INFO ] Bucket 'test-bucket' exists and is accessible
2026-05-19 10:15:31 [INFO ] Bucket validation successful
2026-05-19 10:15:31 [INFO ] Application shutdown complete

=================================
S3 / MinIO File Management System
=================================

1. Create Master Folder - Bas-887766
...

Enter your choice: 1
Creating master folder: Bas-887766/
SUCCESS: Operation completed successfully.
Press Enter to continue...
```

## Error Handling

The application handles the following error scenarios gracefully:

- Missing config.properties file
- Invalid configuration values
- Invalid credentials
- Bucket not found
- Network failures
- SSL/TLS failures
- Timeout errors
- File not found
- Access denied

All errors produce readable, enterprise-style error messages and do not crash the application unexpectedly.

## Logging

The application uses Microsoft.Extensions.Logging with the following log levels:

- **DEBUG**: Detailed operation information
- **INFO**: Operation success/failure
- **WARNING**: Non-critical issues (e.g., invalid property lines)
- **ERROR**: Operation failures
- **CRITICAL**: Unhandled exceptions

Log format: `yyyy-MM-dd HH:mm:ss [LEVEL] Message`

## S3 Key Structure

The application uses S3 prefixes to emulate folders:

```
Bas-887766/
Bas-887766/sub_folder_1/
Bas-887766/sub_folder_2/
Bas-887766/sub_folder_1/File_1.txt
Bas-887766/sub_folder_1/File_2.txt
Bas-887766/sub_folder_2/File_1.txt
```

## Dependencies

- AWSSDK.S3 (3.7.307.9)
- Microsoft.Extensions.Logging (8.0.0)
- Microsoft.Extensions.Logging.Console (8.0.0)
- Microsoft.Extensions.Logging.Abstractions (8.0.0)

## License

This is a production-ready enterprise application for S3-compatible storage management.