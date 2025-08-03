using XivFatalFrame.ScreenshotDatabasing;

namespace XivFatalFrame.Screenshotter;

internal class ScreenshotElement
{
    public          double           Timer;
    public readonly ScreenshotParams Params;

    public ScreenshotElement(double timer, ScreenshotParams @params)
    {
        Timer  = timer;
        Params = @params;
    }
}
