using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Infrastructure;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using Mi5hmasH.Logger;
using Mi5hmasH.Progress;

namespace AutostrongSharpCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    /// <summary>
    /// Gets the default instance of the automatic strong decryption utility for processing encrypted data.
    /// </summary>
    public AutoStrongDeencryptor Deencryptor { get; } = new();
    
    /// <summary>
    /// Creates a new ParallelOptions instance configured with the specified cancellation token and an optimal degree of parallelism for the current environment.
    /// </summary>
    /// <param name="cts">The CancellationTokenSource whose token will be used to support cancellation of parallel operations.</param>
    /// <returns>A ParallelOptions object initialized with the provided cancellation token and a maximum degree of parallelism based on the number of available processors.</returns>
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts)
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    /// <summary>
    /// Marks the progress reporting as complete by reporting 100% progress.
    /// </summary>
    /// <param name="progressTracker">The progress tracker used to report progress.</param>
    /// <param name="errorCounter">The error counter used to report errors.</param>
    private void LogAllTasksCompleted(ProgressTracker progressTracker, ErrorCounter errorCounter)
        => logger.LogInfo($"{progressTracker} All tasks completed. {errorCounter}");

    /// <summary>
    /// Asynchronously decrypts all files in the specified input directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to decrypt.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the decryption operation.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    public async Task DecryptFilesAsync(string inputDir, CancellationTokenSource cts)
        => await Task.Run(() => DecryptFiles(inputDir, cts));

    /// <summary>
    /// Decrypts all encrypted files in the specified input directory and saves the decrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be decrypted. Only files with the expected encrypted file extension are processed.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the decryption operation. If cancellation is requested, the method will stop processing remaining files.</param>
    public void DecryptFiles(string inputDir, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // DECRYPT
        logger.LogInfo($"Decrypting [{progressTracker.Total}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("decrypted");
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}", group);
                        break;
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try
                    {
                        autoStrongFile.SetFileData(data, true);
                        if (!autoStrongFile.IsEncrypted)
                        {
                            errorCounter.AddWarning($"{progressTracker} The [{fileName}] file is not encrypted, skipping...", group);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to process the [{fileName}] file data: {ex}", group);
                        break;
                    }
                    // Try to decrypt file data
                    try
                    {
                        logger.LogInfo($"{progressTracker} Decrypting the [{fileName}] file...", group);
                        autoStrongFile.DecryptFile();
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to decrypt the file: {ex.Message}", group);
                        break;
                    }
                    // Try to save the decrypted file data
                    try
                    {
                        var outputFilePath = Path.Combine(outputDir, fileName);
                        var outputData = autoStrongFile.GetFileData();
                        File.WriteAllBytes(outputFilePath, outputData);
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to save the file: {ex}", group);
                        break;
                    }
                    logger.LogInfo($"{progressTracker} Decrypted the [{fileName}] file.", group);
                    break;
                }
                progressTracker.Increment();
                progressReporter.Report(progressTracker.Percentage);
            });
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }

    /// <summary>
    /// Asynchronously encrypts all files in the specified directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to encrypt.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the encryption operation. If cancellation is requested, the operation will terminate early.</param>
    /// <returns>A task that represents the asynchronous encryption operation.</returns>
    public async Task EncryptFilesAsync(string inputDir, CancellationTokenSource cts)
        => await Task.Run(() => EncryptFiles(inputDir, cts));

    /// <summary>
    /// Encrypts all files with the specified extension in the given input directory and saves the encrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be encrypted. Only files matching the required extension are processed.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the encryption operation. If cancellation is requested, the process will terminate early.</param>
    public void EncryptFiles(string inputDir, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // ENCRYPT
        logger.LogInfo($"Encrypting [{progressTracker.Total}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("encrypted");
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}", group);
                        break;
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try
                    {
                        autoStrongFile.SetFileData(data);
                        if (autoStrongFile.IsEncrypted)
                        {
                            errorCounter.AddWarning($"{progressTracker} The [{fileName}] file is already encrypted, skipping...", group);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to process the [{fileName}] file data: {ex}", group);
                        break;
                    }
                    // Try to encrypt file data
                    try
                    {
                        logger.LogInfo($"{progressTracker} Encrypting the [{fileName}] file...", group);
                        autoStrongFile.EncryptFile();
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to encrypt the file: {ex.Message}", group);
                        break;
                    }
                    // Try to save the encrypted file data
                    try
                    {
                        var outputFilePath = Path.Combine(outputDir, fileName);
                        var outputData = autoStrongFile.GetFileData();
                        File.WriteAllBytes(outputFilePath, outputData);
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to save the file: {ex}", group);
                        break;
                    }
                    logger.LogInfo($"{progressTracker} Encrypted the [{fileName}] file.", group);
                    break;
                }
                progressTracker.Increment();
                progressReporter.Report(progressTracker.Percentage);
            });
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }

    /// <summary>
    /// Asynchronously re-signs all eligible files in the specified input directory for the given user.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be re-signed. Must not be null or empty.</param>
    /// <param name="userId">The identifier of the user for whom the files are being re-signed.</param>
    /// <param name="cts">A cancellation token source that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous re-sign operation.</returns>
    public async Task ResignFilesAsync(string inputDir, uint userId, CancellationTokenSource cts)
        => await Task.Run(() => ResignFiles(inputDir, userId, cts));

    /// <summary>
    /// Processes and re-signs all eligible files in the specified input directory for the given user.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be re-signed. Only files with the required extension are processed.</param>
    /// <param name="userId">The user ID to assign to each re-signed file.</param>
    /// <param name="cts">A cancellation token source that can be used to cancel the operation before completion.</param>
    public void ResignFiles(string inputDir, uint userId, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // RE-SIGN
        logger.LogInfo($"Re-signing [{progressTracker.Total}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserIdAndSuffix(userId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}", group);
                        break;
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try { autoStrongFile.SetFileData(data); }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to process the [{fileName}] file data: {ex}", group);
                        break;
                    }
                    autoStrongFile.DataHeader.SetUserId(userId, Deencryptor);
                    if (!autoStrongFile.IsEncrypted)
                    {
                        // Try to encrypt file data
                        try
                        {
                            logger.LogInfo($"{progressTracker} Encrypting the [{fileName}] file...", group);
                            autoStrongFile.EncryptFile();
                        }
                        catch (Exception ex)
                        {
                            errorCounter.AddError($"{progressTracker} Failed to encrypt the file: {ex.Message}", group);
                            break;
                        }
                    }
                    // Try to save the encrypted file data
                    try
                    {
                        var outputFilePath = Path.Combine(outputDir, fileName);
                        var outputData = autoStrongFile.GetFileData();
                        File.WriteAllBytes(outputFilePath, outputData);
                    }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to save the file: {ex}", group);
                        break;
                    }
                    logger.LogInfo($"{progressTracker} Re-signed the [{fileName}] file.", group);
                    break;
                }
                progressTracker.Increment();
                progressReporter.Report(progressTracker.Percentage);
            });
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the user ID of the current owner from the first available AutoStrong file in the specified directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing AutoStrong files to process. Cannot be null or empty.</param>
    /// <returns>The user ID of the current owner if a valid AutoStrong file is found and processed successfully; otherwise, <see langword="null"/>.</returns>
    public async Task<uint?> GetCurrentOwnerAsync(string inputDir)
        => await Task.Run(() => GetCurrentOwner(inputDir));

    /// <summary>
    /// Retrieves the user ID of the current owner from the first available AutoStrong file in the specified directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing AutoStrong files to process. Cannot be null or empty.</param>
    /// <returns>The user ID of the current owner if a valid AutoStrong file is found and processed successfully; otherwise, <see langword="null"/>.</returns>
    public uint? GetCurrentOwner(string inputDir)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return null;
        }
        var file = filesToProcess[0];
        var fileName = Path.GetFileName(file);
        // Try to read file data
        byte[] data;
        try { data = File.ReadAllBytes(file); }
        catch (Exception ex)
        {
            logger.LogError($"Failed to read the [{fileName}] file: {ex}");
            return null;
        }
        // Process file data
        var autoStrongFile = new AutoStrongFile(Deencryptor);
        try { autoStrongFile.SetFileData(data); }
        catch (Exception ex)
        {
            logger.LogError($"Failed to process the [{fileName}] file data: {ex}");
            return null;
        }
        // Try to get user ID
        try
        {
            var userId = autoStrongFile.DataHeader.GetUserId(Deencryptor);
            logger.LogInfo($"UserId has been found: {userId}.");
            // Return user ID
            return userId;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to get the UserId from the [{fileName}]: {ex}");
        }
        return null;
    }
}