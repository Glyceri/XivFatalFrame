using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;

public class QuestParameters : ScreenshotParams
{
    public override ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.QuestCompletion;

    public uint QuestId { get; set; }

    public QuestParameters() { }

    public QuestParameters(BasicScreenshotData screenshotData, uint questId) : base(screenshotData)
    {
        QuestId = questId;
    }

    public override string ToString() => $"[{ScreenshotReason}] [{QuestId}]";
}
