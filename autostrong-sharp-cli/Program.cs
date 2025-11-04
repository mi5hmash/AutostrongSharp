using AutostrongSharpCore;
using AutostrongSharpCore.GameProfile;
using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Infrastructure;
using Mi5hmasH.AppInfo;
using Mi5hmasH.ConsoleHelper;
using Mi5hmasH.GameProfile;
using Mi5hmasH.Logger;
using Mi5hmasH.Logger.Models;
using Mi5hmasH.Logger.Providers;

#region SETUP

// CONSTANTS
const string breakLine = "---";

// Initialize APP_INFO
var appInfo = new MyAppInfo("autostrong-sharp-cli");

// Initialize LOGGER
var logger = new SimpleLogger
{
    LoggedAppName = appInfo.Name
};
// Configure ConsoleLogProvider
var consoleLogProvider = new ConsoleLogProvider();
logger.AddProvider(consoleLogProvider);
// Configure FileLogProvider
var fileLogProvider = new FileLogProvider(MyAppInfo.RootPath, 2);
fileLogProvider.CreateLogFile();
logger.AddProvider(fileLogProvider);
// Add event handler for unhandled exceptions
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    if (e.ExceptionObject is not Exception exception) return;
    var logEntry = new LogEntry(SimpleLogger.LogSeverity.Critical, $"Unhandled Exception: {exception}");
    fileLogProvider.Log(logEntry);
    fileLogProvider.Flush();
};
// Flush log providers on process exit
AppDomain.CurrentDomain.ProcessExit += (_, _) => logger.Flush();

//Initialize ProgressReporter
var progressReporter = new ProgressReporter(new Progress<string>(Console.WriteLine), null);

// Initialize CORE
var core = new Core(logger, progressReporter);

// Print HEADER
ConsoleHelper.PrintHeader(appInfo, breakLine);

// Say HELLO
ConsoleHelper.SayHello(breakLine);

// Get ARGUMENTS from command line
#if DEBUG
// For debugging purposes, you can manually set the arguments...
if (args.Length < 1)
{
    // ...below
    const string localArgs = "-m TEST";
    args = ConsoleHelper.GetArgs(localArgs);
}
#endif
var arguments = ConsoleHelper.ReadArguments(args);
#if DEBUG
// Write the arguments to the console for debugging purposes
ConsoleHelper.WriteArguments(arguments);
Console.WriteLine(breakLine);
#endif

#endregion

#region MAIN

// Show HELP if no arguments are provided or if -h is provided
if (arguments.Count == 0 || arguments.ContainsKey("-h"))
{
    PrintHelp();
    return;
}

// Optional argument: isVerbose
var isVerbose = arguments.ContainsKey("-v");

// Get MODE
arguments.TryGetValue("-m", out var mode);
switch (mode)
{
    case "decrypt" or "d":
        DecryptAll();
        break;
    case "encrypt" or "e":
        EncryptAll();
        break;
    case "resign" or "r":
        ResignAll();
        break;
    default:
        throw new ArgumentException($"Unknown mode: '{mode}'.");
}

// EXIT the application
Console.WriteLine(breakLine); // print a break line
ConsoleHelper.SayGoodbye(breakLine);
#if DEBUG
ConsoleHelper.PressAnyKeyToExit();
#else
if (isVerbose) ConsoleHelper.PressAnyKeyToExit();
#endif

return;

#endregion

#region HELPERS

static void PrintHelp()
{
    const string userId = "1";
    var inputPath = Path.Combine(".", "InputDirectory");
    var gameProfilePath = Path.Combine(".", "game_profile.bin");
    var exeName = Path.Combine(".", Path.GetFileName(Environment.ProcessPath) ?? "ThisExecutableFileName.exe");
    var helpMessage = $"""
                       Usage: {exeName} -m <mode> [options]

                       Modes:
                         -m d  Decrypt SaveData files
                         -m e  Encrypt SaveData files
                         -m r  Re-sign SaveData files

                       Options:
                         -g <game_profile_path>  Path to the Game Profile file
                         -p <input_folder_path>  Path to folder containing SaveData files
                         -u <user_id>            User ID (used in re-sign mode)
                         -v                      Verbose output
                         -h                      Show this help message

                       Examples:
                         Decrypt:  {exeName} -m d -g "{gameProfilePath}" -p "{inputPath}"
                         Encrypt:  {exeName} -m e -g "{gameProfilePath}" -p "{inputPath}"
                         Re-sign:  {exeName} -m r -g "{gameProfilePath}" -p "{inputPath}" -u {userId}
                       """;
    Console.WriteLine(helpMessage);
}

string GetValidatedInputRootPath()
{
    arguments.TryGetValue("-p", out var inputRootPath);
    if (File.Exists(inputRootPath)) inputRootPath = Path.GetDirectoryName(inputRootPath);
    return !Directory.Exists(inputRootPath)
        ? throw new DirectoryNotFoundException($"The provided path '{inputRootPath}' is not a valid directory or does not exist.")
        : inputRootPath;
}

void LoadGameProfile()
{
    arguments.TryGetValue("-g", out var gameProfilePath);
    if (string.IsNullOrEmpty(gameProfilePath))
        throw new ArgumentException("Game Profile file is missing.");
    var gameProfileManager = new GameProfileManager<AutostrongGameProfile>();
    gameProfileManager.SetEncryptor(Keychain.GpMagic);
    gameProfileManager.Load(gameProfilePath);
    // Configure Deencryptor
    core.Deencryptor.Configure(gameProfileManager.GameProfile.EncryptionKey?.FromBase64<uint>(), gameProfileManager.GameProfile.Sbox?.FromBase64<uint>());
}

#endregion

#region MODES

void DecryptAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    LoadGameProfile();
    core.DecryptFiles(inputRootPath, cts);
    cts.Dispose();
}

void EncryptAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    LoadGameProfile();
    core.EncryptFiles(inputRootPath, cts);
    cts.Dispose();
}

void ResignAll()
{
    var cts = new CancellationTokenSource();
    arguments.TryGetValue("-u", out var userId);
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("Output User ID is missing.");
    var inputRootPath = GetValidatedInputRootPath();
    LoadGameProfile();
    core.ResignFiles(inputRootPath, Convert.ToUInt32(userId), cts);
    cts.Dispose();
}

#endregion