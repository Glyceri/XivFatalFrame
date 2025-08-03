using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class AchievementParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Achievement;

    public uint AchievementId { get; set; }

    public AchievementParameters() { }

    public AchievementParameters(uint achievementId)
    {
        AchievementId = achievementId;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{AchievementId}]";
}
