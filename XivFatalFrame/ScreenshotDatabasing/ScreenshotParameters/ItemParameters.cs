using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class ItemParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.ItemUnlocked;

    public uint ItemId { get; set; }

    public ItemParameters() { }

    public ItemParameters(uint itemId)
    {
        ItemId = itemId;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{ItemId}]";
}
