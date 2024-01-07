using static AutostrongSharpCore.Helpers.DsssGameProfileService;

namespace AutostrongSharpCore.Models.GameProfile;

public class DsssGameProfile
{
    /// <summary>
    /// The title of a game.
    /// </summary>
    public string GameTitle { get; set; } = "Unknown";

    /// <summary>
    /// AppIcon encoded with Base64.
    /// </summary>
    public string Base64AppIcon { get; set; } = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAclBMVEUAAAD6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRO9kW18AAAAJXRSTlMAvkD40IEJ/GnklCsfEe2qn45b3mPziXduUToMyLaxpp1UTjMltzRUGQAAAOtJREFUOMvNk7kCgjAQREeEEC4V8EDwVv7/F9UsiRMsaCx8DcxmSPYI+Blq1nvMFHyifkQEj3w+NsxzMMv+iyWIYAjW5VpHh0EEZAhlVy2qrY0M4dDyyQ3AvVVAvjFa2/ViYfQJUK+tkgewNYFFASGVDSpg936WwF4iqaxnscg1kAwduEokzozB9nBTmrewA862n1yijRXmBCp15a0fFVDFTq7GhqQDGqfEEIxGVLEOKElbScgJUZnCBdlH2DKRchI895RbLdTVThxeq6E5qdIJTeMmgxzI4+ZSk2YbO/PUlZu8tFPXfvrH+R+eloxHObKCkJYAAAAASUVORK5CYII=";

    /// <summary>
    /// Steam AppID.
    /// </summary>
    public uint SteamAppId { get; set; }

    /// <summary>
    /// Platform (0:PC; 1:PS4; 2:XboxOne;).
    /// </summary>
    public uint Platform { get; set; } = (uint)PlatformsEnum.Pc;
}