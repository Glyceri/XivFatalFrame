using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class SightseeingParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.SightseeingLog;

    public ushort VistaIndex { get; set; }

    public SightseeingParameters() { }

    public SightseeingParameters(ushort vistaIndex)
    {
        VistaIndex = vistaIndex;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{VistaIndex}]";
}
