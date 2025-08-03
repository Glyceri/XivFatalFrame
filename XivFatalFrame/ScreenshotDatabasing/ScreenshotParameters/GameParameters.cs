using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class GameParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.GAME;

    public GameParameters() { }

    public override string ToString() => $"[{ScreenshotReason}]";
}
