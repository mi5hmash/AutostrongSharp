using AutostrongSharpCore;
using AutostrongSharpCore.GameProfile;
using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Infrastructure;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using Mi5hmasH.GameProfile;
using Mi5hmasH.Logger;

namespace QualityControl.xUnit;

public sealed class AutostrongSharpCoreTests : IDisposable
{
    private readonly Core _core;
    private readonly GameProfileManager<AutostrongGameProfile> _gameProfileManager = new();
    private readonly ITestOutputHelper _output;
    
    public AutostrongSharpCoreTests(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("SETUP");

        // Setup
        var logger = new SimpleLogger();
        var progressReporter = new ProgressReporter(null, null);
        _core = new Core(logger, progressReporter);
    }

    public void Dispose()
    {
        _output.WriteLine("CLEANUP");
    }
    
    [Fact]
    public void DecryptFiles_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            _core.DecryptFiles(tempDir, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void EncryptFiles_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            _core.EncryptFiles(tempDir, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void ResignFiles_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            _core.ResignFiles(tempDir, 1, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }
    
    private void LoadGameProfile()
    {
        // Load GameProfile
        _gameProfileManager.SetEncryptor(Keychain.SettingsMagic);
        _gameProfileManager.Load(Properties.Resources.profileFile, "profile");
        // Copy EncryptionKey and SBox to Deencryptor
        _core.Deencryptor.Configure(_gameProfileManager.GameProfile.EncryptionKey?.FromBase64<uint>(), _gameProfileManager.GameProfile.Sbox?.FromBase64<uint>());
    }

    [Fact]
    public void DecryptFiles_DoesDecrypt()
    {
        // Arrange
        LoadGameProfile();
        var autoStrongFile = new AutoStrongFile(_core.Deencryptor);
        autoStrongFile.SetFileData(Properties.Resources.encryptedFile);

        // Act
        autoStrongFile.DecryptFile();
        var resultData = autoStrongFile.GetFileData();

        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)resultData);
    }
    
    [Fact]
    public void EncryptFiles_DoesEncrypt()
    {
        // Arrange
        LoadGameProfile();
        var autoStrongFile = new AutoStrongFile(_core.Deencryptor);
        autoStrongFile.SetFileData(Properties.Resources.decryptedFile);

        // Act
        autoStrongFile.EncryptFile();
        var resultData = autoStrongFile.GetFileData();

        // Assert
        Assert.Equal(Properties.Resources.encryptedFile, (ReadOnlySpan<byte>)resultData);
    }

    [Fact]
    public void ResignFiles_DoesResign()
    {
        // Arrange
        LoadGameProfile();
        var autoStrongFile = new AutoStrongFile(_core.Deencryptor);
        autoStrongFile.SetFileData(Properties.Resources.encryptedFile);
        const uint expectedUserId = 1911;

        // Act
        autoStrongFile.DataHeader.SetUserId(expectedUserId, _core.Deencryptor);
        var resultData = autoStrongFile.DataHeader.GetUserId(_core.Deencryptor);

        // Assert
        Assert.Equal(expectedUserId, resultData);
    }
}