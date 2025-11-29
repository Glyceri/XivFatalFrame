using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class LevelUpParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.LevelUp;

    public byte NewLevel { get; set; }

    public LevelUpParameters() { }

    public LevelUpParameters(BasicScreenshotData screenshotData, byte classId, byte newLevel) : base(screenshotData)
    {
        NewLevel = newLevel;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{ClassId}] [{NewLevel}]";
}