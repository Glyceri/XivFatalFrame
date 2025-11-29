using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class DutyParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.DutyCompletion;

    public ushort DutyId { get; set; }

    public DutyParameters() { }

    public DutyParameters(BasicScreenshotData screenshotData, ushort dutyId) : base(screenshotData)
    {
        DutyId  = dutyId;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{DutyId}] [{ClassId}] [{Level}]";
}
