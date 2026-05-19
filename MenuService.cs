using Microsoft.Extensions.Logging;

namespace S3FileManager;

/// <summary>
/// Handles menu-driven console operations for S3 file management.
/// </summary>
public class MenuService
{
    private readonly ILogger<MenuService> _logger;
    private readonly ConsoleHelper _consoleHelper;
    private readonly S3StorageService _storageService;
    private bool _isRunning;

    public MenuService(
        ILogger<MenuService> logger,
        ConsoleHelper consoleHelper,
        S3StorageService storageService)
    {
        _logger = logger;
        _consoleHelper = consoleHelper;
        _storageService = storageService;
        _isRunning = true;
    }

    /// <summary>
    /// Starts the menu-driven interaction loop.
    /// Continues until user selects Exit or closes terminal.
    /// </summary>
    public async Task RunMenuLoopAsync()
    {
        _logger.LogInformation("Starting menu service");

        while (_isRunning)
        {
            try
            {
                DisplayMenu();
                var choice = _consoleHelper.ReadLine().Trim();

                await ProcessChoiceAsync(choice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing menu choice");
                _consoleHelper.DisplayError($"An error occurred: {ex.Message}");
                _consoleHelper.WaitForContinue();
            }
        }
    }

    /// <summary>
    /// Displays the main menu to the console.
    /// </summary>
    private void DisplayMenu()
    {
        _consoleHelper.Clear();
        _consoleHelper.Write(Constants.MenuHeader);
    }

    /// <summary>
    /// Processes the user's menu choice.
    /// </summary>
    private async Task ProcessChoiceAsync(string choice)
    {
        switch (choice)
        {
            case "1":
                await CreateMasterFolderAsync();
                break;
            case "2":
                await CreateSubFolder1Async();
                break;
            case "3":
                await CreateSubFolder2Async();
                break;
            case "4":
                await UploadFile1Async();
                break;
            case "5":
                await UploadFile2Async();
                break;
            case "6":
                await CopyFile1ToSubFolder2Async();
                break;
            case "7":
                await MoveFile2ToSubFolder1Async();
                break;
            case "8":
                await DeleteFile1FromSubFolder1Async();
                break;
            case "9":
                await ListAllFilesAndFoldersAsync();
                break;
            case "10":
                await DownloadAllFilesAsync();
                break;
            case "0":
                ExitApplication();
                break;
            default:
                _consoleHelper.DisplayError(Constants.InvalidChoiceMessage);
                _consoleHelper.WaitForContinue();
                break;
        }
    }

    /// <summary>
    /// Menu option 1: Create master folder (Bas-887766/)
    /// </summary>
    private async Task CreateMasterFolderAsync()
    {
        _consoleHelper.WriteLine("Creating master folder: Bas-887766/");
        await _storageService.CreateFolderAsync(Constants.MasterFolderPrefix);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 2: Create sub-folder 1
    /// </summary>
    private async Task CreateSubFolder1Async()
    {
        _consoleHelper.WriteLine($"Creating sub folder: {Constants.SubFolder1Prefix}");
        await _storageService.CreateFolderAsync(Constants.SubFolder1Prefix);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 3: Create sub-folder 2
    /// </summary>
    private async Task CreateSubFolder2Async()
    {
        _consoleHelper.WriteLine($"Creating sub folder: {Constants.SubFolder2Prefix}");
        await _storageService.CreateFolderAsync(Constants.SubFolder2Prefix);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 4: Upload File_1.txt to sub-folder 1
    /// </summary>
    private async Task UploadFile1Async()
    {
        var localPath = PathHelper.GetFilePath(Constants.File1Name);
        _consoleHelper.WriteLine($"Uploading {Constants.File1Name} to {Constants.SubFolder1Prefix}{Constants.File1Name}");
        await _storageService.UploadFileAsync(localPath, Constants.File1InSubFolder1);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 5: Upload File_2.txt to sub-folder 2
    /// </summary>
    private async Task UploadFile2Async()
    {
        var localPath = PathHelper.GetFilePath(Constants.File2Name);
        _consoleHelper.WriteLine($"Uploading {Constants.File2Name} to {Constants.SubFolder2Prefix}{Constants.File2Name}");
        await _storageService.UploadFileAsync(localPath, Constants.File2InSubFolder2);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 6: Copy File_1.txt to sub-folder 2 (server-side copy)
    /// </summary>
    private async Task CopyFile1ToSubFolder2Async()
    {
        _consoleHelper.WriteLine($"Copying {Constants.File1InSubFolder1} to {Constants.File1InSubFolder2}");
        await _storageService.CopyObjectAsync(Constants.File1InSubFolder1, Constants.File1InSubFolder2);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 7: Move File_2.txt to sub-folder 1 (copy then delete)
    /// </summary>
    private async Task MoveFile2ToSubFolder1Async()
    {
        _consoleHelper.WriteLine($"Moving {Constants.File2InSubFolder2} to {Constants.File2InSubFolder1}");
        await _storageService.MoveObjectAsync(Constants.File2InSubFolder2, Constants.File2InSubFolder1);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 8: Delete File_1.txt from sub-folder 1
    /// </summary>
    private async Task DeleteFile1FromSubFolder1Async()
    {
        _consoleHelper.WriteLine($"Deleting {Constants.File1InSubFolder1}");
        await _storageService.DeleteObjectAsync(Constants.File1InSubFolder1);
        _consoleHelper.DisplaySuccess(Constants.OperationSuccessMessage);
        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 9: List all files and folders recursively
    /// </summary>
    private async Task ListAllFilesAndFoldersAsync()
    {
        _consoleHelper.WriteLine("Listing all files and folders recursively:");

        var objects = await _storageService.ListObjectsRecursivelyAsync();

        _consoleHelper.WriteLine();
        if (objects.Count == 0)
        {
            _consoleHelper.WriteLine("No objects found in bucket.");
        }
        else
        {
            foreach (var obj in objects)
            {
                var isDirectory = obj.Key.EndsWith("/");
                var prefix = isDirectory ? Constants.DirectoryListingPrefix : Constants.FileListingPrefix;
                _consoleHelper.WriteLine($"{prefix} {obj.Key}");
            }
            _consoleHelper.WriteLine();
            _consoleHelper.WriteLine($"Total items: {objects.Count}");
        }

        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 10: Download all files to local folder
    /// </summary>
    private async Task DownloadAllFilesAsync()
    {
        var downloadFolder = Path.Combine(AppContext.BaseDirectory, "DownloadedFiles");
        _consoleHelper.WriteLine($"Downloading all files to: {downloadFolder}");

        var count = await _storageService.DownloadAllFilesAsync(downloadFolder);

        if (count > 0)
        {
            _consoleHelper.DisplaySuccess($"Downloaded {count} file(s) to: {downloadFolder}");
        }
        else
        {
            _consoleHelper.DisplayWarning("No files found to download.");
        }

        _consoleHelper.WaitForContinue();
    }

    /// <summary>
    /// Menu option 0: Exit the application
    /// </summary>
    private void ExitApplication()
    {
        _consoleHelper.WriteLine(Constants.ExitMessage);
        _isRunning = false;
    }
}