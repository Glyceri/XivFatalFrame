using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class SightseeingParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.SightseeingLog;

    public ushort VistaIndex { get; set; }

    public SightseeingParameters() { }

    public SightseeingParameters(BasicScreenshotData screenshotData, ushort vistaIndex) : base(screenshotData)
    {
        VistaIndex = vistaIndex;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{VistaIndex}]";
}
