using System.Diagnostics.CodeAnalysis;

namespace AutostrongSharpWpf.Settings;

public class MyAppSettings : IEquatable<MyAppSettings>
{
    public uint UserId { get; set; }
    public bool IsSu { get; set; }

    /// <summary>
    /// Copies the values of user-related settings from the specified <see cref="MyAppSettings"/> instance to the current instance.
    /// </summary>
    /// <param name="other">An instance of <see cref="MyAppSettings"/> whose property values will be assigned to this instance.</param>
    public void Set(MyAppSettings other)
    {
        UserId = other.UserId;
        IsSu = other.IsSu;
    }

    public bool Equals(MyAppSettings? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        return UserId == other.UserId &&
               IsSu == other.IsSu;
    }

    public int GetHashCodeStable()
    {
        var hc = new HashCode();
        // Add fields to the hash code computation
        hc.Add(UserId);
        hc.Add(IsSu);
        return hc.ToHashCode();
    }

    // This is a workaround to avoid the default GetHashCode() implementation in objects where all fields are mutable.
    private readonly Guid _uniqueId = Guid.NewGuid();
    public override int GetHashCode()
        => _uniqueId.GetHashCode();

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is MyAppSettings castedObj && Equals(castedObj);

    public static bool operator ==(MyAppSettings? left, MyAppSettings? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(MyAppSettings? left, MyAppSettings? right) 
        => !(left == right);
}