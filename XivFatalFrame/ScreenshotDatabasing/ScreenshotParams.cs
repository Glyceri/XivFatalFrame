using LiteDB;
using System;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing;

[Serializable]
public class ScreenshotParams
{
    [BsonIgnore]
    public virtual ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Unknown;
    
    public override string ToString() => string.Empty;
}
