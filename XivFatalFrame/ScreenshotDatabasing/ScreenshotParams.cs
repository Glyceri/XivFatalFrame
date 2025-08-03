using LiteDB;
using System;
using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;

namespace XivFatalFrame.ScreenshotDatabasing;

[Serializable]
public class ScreenshotParams
{
    [BsonIgnore]
    public virtual ScreenshotReason ScreenshotReason { get; } = ScreenshotReason.Unknown;

    public uint     ClassId               { get; set; }
    public byte     Level                 { get; set; }
    public uint     CurrentMap            { get; set; }
    public ushort   CurrentTerritoryType  { get; set; }
    public byte     CurrentWeather        { get; set; }
    public long     EorzeanTime           { get; set; }

    public ScreenshotParams() { }

    public ScreenshotParams(BasicScreenshotData screenshotData)
    {
        ClassId                 = screenshotData.ClassId;
        Level                   = screenshotData.Level;
        CurrentMap              = screenshotData.CurrentMap;
        CurrentTerritoryType    = screenshotData.CurrentTerritoryType;
        CurrentWeather          = screenshotData.CurrentWeather;
        EorzeanTime             = screenshotData.EorzeanTime;
    }

    public override string ToString() => string.Empty;
}
