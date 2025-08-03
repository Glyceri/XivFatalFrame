using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class DutyParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.DutyCompletion;

    public ushort DutyId { get; set; }
    public uint ClassId  { get; set; }
    public byte Level    { get; set; }

    public DutyParameters() { }

    public DutyParameters(ushort dutyId, uint classId, byte level)
    {
        DutyId  = dutyId;
        ClassId = classId;
        Level   = level;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{DutyId}] [{ClassId}] [{Level}]";
}
