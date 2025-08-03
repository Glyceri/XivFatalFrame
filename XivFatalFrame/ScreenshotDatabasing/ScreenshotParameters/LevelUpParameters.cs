using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class LevelUpParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.LevelUp;

    public byte ClassId  { get; set; }
    public byte NewLevel { get; set; }

    public LevelUpParameters() { }

    public LevelUpParameters(byte classId, byte newLevel)
    {
        ClassId  = classId;
        NewLevel = newLevel;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{ClassId}] [{NewLevel}]";
}