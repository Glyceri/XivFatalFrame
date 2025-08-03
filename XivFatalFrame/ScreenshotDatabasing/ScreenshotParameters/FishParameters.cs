using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class FishParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Fish;

    public bool IsSpearfish { get; set; }
    public uint FishId      { get; set; }

    public FishParameters() { }

    public FishParameters(bool isSpearfish, uint fishId)
    {
        IsSpearfish = isSpearfish;
        FishId      = fishId;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{IsSpearfish}] [{FishId}]";
}
