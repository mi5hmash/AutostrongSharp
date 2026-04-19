using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Infrastructure;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using Mi5hmasH.Logger;

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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{AutoStrongFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) 
            return;
        // DECRYPT
        logger.LogInfo($"Decrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("decrypted");
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
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
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try
                    {
                        autoStrongFile.SetFileData(data, true);
                        if (!autoStrongFile.IsEncrypted)
                        {
                            logger.LogWarning($"[{progress}/{filesToProcess.Length}] The [{fileName}] file is not encrypted, skipping...", group);
                            break; // Skip to the next file
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to process the [{fileName}] file data: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to decrypt file data
                    try
                    {
                        logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypting the [{fileName}] file...", group);
                        autoStrongFile.DecryptFile();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to decrypt the file: {ex.Message}", group);
                        break; // Skip to the next file
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
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypted the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{AutoStrongFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) 
            return;
        // ENCRYPT
        logger.LogInfo($"Encrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("encrypted");
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
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
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try
                    {
                        autoStrongFile.SetFileData(data);
                        if (autoStrongFile.IsEncrypted)
                        {
                            logger.LogWarning($"[{progress}/{filesToProcess.Length}] The [{fileName}] file is already encrypted, skipping...", group);
                            break; // Skip to the next file
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to process the [{fileName}] file data: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to encrypt file data
                    try
                    {
                        logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                        autoStrongFile.EncryptFile();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to encrypt the file: {ex.Message}", group);
                        break; // Skip to the next file
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
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypted the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{AutoStrongFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) 
            return;
        // RE-SIGN
        logger.LogInfo($"Re-signing [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserIdAndSuffix(userId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
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
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Process file data
                    var autoStrongFile = new AutoStrongFile(Deencryptor);
                    try { autoStrongFile.SetFileData(data); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to process the [{fileName}] file data: {ex}", group);
                        break; // Skip to the next file
                    }
                    autoStrongFile.DataHeader.SetUserId(userId, Deencryptor);
                    if (!autoStrongFile.IsEncrypted)
                    {
                        // Try to encrypt file data
                        try
                        {
                            logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                            autoStrongFile.EncryptFile();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Failed to encrypt the file: {ex.Message}", group);
                            break; // Skip to the next file
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
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Re-signed the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{AutoStrongFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) 
            return null;
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