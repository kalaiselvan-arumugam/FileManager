namespace S3FileManager;

/// <summary>
/// Application-wide constants for S3 operations and folder structure.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Master folder name for the application
    /// </summary>
    public const string MasterFolderName = "Bas-887766";

    /// <summary>
    /// First sub-folder name
    /// </summary>
    public const string SubFolder1Name = "sub_folder_1";

    /// <summary>
    /// Second sub-folder name
    /// </summary>
    public const string SubFolder2Name = "sub_folder_2";

    /// <summary>
    /// First file name to upload
    /// </summary>
    public const string File1Name = "File_1.txt";

    /// <summary>
    /// Second file name to upload
    /// </summary>
    public const string File2Name = "File_2.txt";

    /// <summary>
    /// Full path to master folder (S3 prefix)
    /// </summary>
    public static string MasterFolderPrefix => MasterFolderName + "/";

    /// <summary>
    /// Full path to sub-folder 1
    /// </summary>
    public static string SubFolder1Prefix => MasterFolderName + "/" + SubFolder1Name + "/";

    /// <summary>
    /// Full path to sub-folder 2
    /// </summary>
    public static string SubFolder2Prefix => MasterFolderName + "/" + SubFolder2Name + "/";

    /// <summary>
    /// S3 key for File_1.txt in sub-folder 1
    /// </summary>
    public static string File1InSubFolder1 => SubFolder1Prefix + File1Name;

    /// <summary>
    /// S3 key for File_2.txt in sub-folder 2
    /// </summary>
    public static string File2InSubFolder2 => SubFolder2Prefix + File2Name;

    /// <summary>
    /// S3 key for File_1.txt in sub-folder 2 (after copy)
    /// </summary>
    public static string File1InSubFolder2 => SubFolder2Prefix + File1Name;

    /// <summary>
    /// S3 key for File_2.txt in sub-folder 1 (after move)
    /// </summary>
    public static string File2InSubFolder1 => SubFolder1Prefix + File2Name;

    /// <summary>
    /// Menu header text
    /// </summary>
    public const string MenuHeader = @"
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
10. Download All Files to Local Folder
0. Exit

Enter your choice: ";

    /// <summary>
    /// Operation success message
    /// </summary>
    public const string OperationSuccessMessage = "Operation completed successfully.";

    /// <summary>
    /// Continue prompt message
    /// </summary>
    public const string ContinuePrompt = "Press Enter to continue...";

    /// <summary>
    /// Invalid menu choice message
    /// </summary>
    public const string InvalidChoiceMessage = "Invalid choice. Please enter a valid option.";

    /// <summary>
    /// Exit message
    /// </summary>
    public const string ExitMessage = "Exiting application. Goodbye!";

    /// <summary>
    /// Directory listing prefix in output
    /// </summary>
    public const string DirectoryListingPrefix = "[DIR ]";

    /// <summary>
    /// File listing prefix in output
    /// </summary>
    public const string FileListingPrefix = "[FILE]";
}