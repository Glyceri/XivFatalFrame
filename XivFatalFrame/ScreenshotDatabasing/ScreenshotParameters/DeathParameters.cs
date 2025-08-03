using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class DeathParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Death;

    public uint     ClassId           { get; set; }
    public byte     Level             { get; set; }
    public ushort   CurrentTerritory  { get; set; }
    public string?  SourceName        { get; set; }
    public uint?    Action            { get; set; }
    public uint?    Amount            { get; set; }

    public DeathParameters() { }

    public DeathParameters(uint classId, byte level, ushort currentTerritory, string? sourceName, uint? action, uint? amount)
    {
        ClassId             = classId;
        Level               = level;
        CurrentTerritory    = currentTerritory;
        SourceName          = sourceName;
        Action              = action;
        Amount              = amount;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{ClassId}] [{Level}] [{CurrentTerritory}]";
}