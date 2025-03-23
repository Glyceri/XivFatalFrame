namespace XivFatalFrame;

internal class ScreenshotElement
{
    public double Timer;
    public readonly ScreenshotReason Reason;

    public ScreenshotElement(double timer, ScreenshotReason reason)
    {
        Timer = timer;
        Reason = reason;
    }
}
