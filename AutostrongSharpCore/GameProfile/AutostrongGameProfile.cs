using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AutostrongSharpCore.GamingPlatforms;
using Mi5hmasH.GameProfile;

namespace AutostrongSharpCore.GameProfile;

public class AutostrongGameProfile : IEquatable<AutostrongGameProfile>, INotifyPropertyChanged, IGameProfile
{
    /// <summary>
    /// Gets or sets metadata information related to the game profile.
    /// </summary>
    public GameProfileMeta Meta { get; set; } = new("AutostrongDSSS", new Version(1, 0, 0, 0));

    /// <summary>
    /// Gets or sets the title of a game.
    /// </summary>
    public string? GameTitle
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(GameTitle));
        }
    }

    /// <summary>
    /// Gets or sets the GamingPlatform.
    /// </summary>
    public GamingPlatform Platform
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(Platform));
        }
    } = GamingPlatform.Other;

    /// <summary>
    /// Gets or sets the application identifier associated with this instance.
    /// </summary>
    public string? AppId
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(AppId));
        }
    }

    /// <summary>
    /// Gets or sets the Game Profile Icon encoded with Base64.
    /// </summary>
    public string? Base64GpIcon
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(Base64GpIcon));
        }
    }

    /// <summary>
    /// Gets or sets the encryption key.
    /// </summary>
    public string? EncryptionKey
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(EncryptionKey));
        }
    }

    /// <summary>
    /// Gets or sets the Sbox.
    /// </summary>
    public string? Sbox
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(Sbox));
        }
    }

    /// <summary>
    /// Copies the game profile data from the specified object if it is an instance of AutostrongGameProfile.
    /// </summary>
    /// <param name="other">The object from which to copy game profile data.</param>
    public void Set(object other)
    {
        if (other is not AutostrongGameProfile profile) return;
        GameTitle = profile.GameTitle;
        AppId = profile.AppId;
        Base64GpIcon = profile.Base64GpIcon;
        Platform = profile.Platform;
        EncryptionKey = profile.EncryptionKey;
        Sbox = profile.Sbox;
    }

    public bool Equals(AutostrongGameProfile? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        var sc = StringComparer.Ordinal;
        return Meta.Equals(other.Meta) &&
               sc.Equals(GameTitle, other.GameTitle) &&
               sc.Equals(AppId, other.AppId) &&
               sc.Equals(Base64GpIcon, other.Base64GpIcon) &&
               Platform == other.Platform &&
               sc.Equals(EncryptionKey, other.EncryptionKey) &&
               sc.Equals(Sbox, other.Sbox);
    }

    public int GetHashCodeStable()
    {
        var hc = new HashCode();
        var sc = StringComparer.Ordinal;
        // Add fields to the hash code computation
        hc.Add(Meta);
        hc.Add(GameTitle, sc);
        hc.Add(AppId, sc);
        hc.Add(Base64GpIcon, sc);
        hc.Add(Platform);
        hc.Add(EncryptionKey, sc);
        hc.Add(Sbox, sc);
        return hc.ToHashCode();
    }

    // This is a workaround to avoid the default GetHashCode() implementation in objects where all fields are mutable.
    private readonly Guid _uniqueId = Guid.NewGuid();
    
    public override int GetHashCode()
        => _uniqueId.GetHashCode();

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is AutostrongGameProfile castedObj && Equals(castedObj);

    public static bool operator ==(AutostrongGameProfile? left, AutostrongGameProfile? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(AutostrongGameProfile? left, AutostrongGameProfile? right) 
        => !(left == right);

    // MVVM support
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}