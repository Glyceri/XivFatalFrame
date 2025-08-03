using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class DeathParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Death;

    public string   SourceName        { get; set; } = string.Empty;
    public uint     Action            { get; set; }
    public uint     Amount            { get; set; }

    public DeathParameters() { }

    public DeathParameters(BasicScreenshotData screenshotData, string sourceName, uint action, uint amount) : base(screenshotData)
    {
        SourceName          = sourceName;
        Action              = action;
        Amount              = amount;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{ClassId}] [{Level}] [{CurrentMap}] [{SourceName}] [{Action}] [{Amount}]";
}