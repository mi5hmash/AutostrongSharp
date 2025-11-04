namespace AutostrongSharpCore.Infrastructure;

/// <summary>
/// Provides constant values used as cryptographic keys or identifiers within the application.
/// </summary>
/// <remarks>
/// If a program can decrypt something, then the user can as well because the program runs on their machine.
/// </remarks>
public static class Keychain
{
    public const string GpMagic = "Yt8m4jLXFBdBG7RepOT0xuKLIGqi/r7PmgnPhRqKwJw=";
    public const string SettingsMagic = "qx04QqHuU9JUNAqv62/iFYZljv9xqAM6LN5NycEGTpk=";
}