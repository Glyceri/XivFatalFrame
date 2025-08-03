using System;
using LiteDB;

namespace XivFatalFrame.ScreenshotDatabasing;

[Serializable]
public class DatabaseEntry
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public DateTime         ScreenshotTime      { get; set; } = DateTime.Now;
    public string           ScreenshotPath      { get; set; } = string.Empty;
    public ScreenshotParams ScreenshotParams    { get; set; } = null!;

    public DatabaseEntry() { }

    public DatabaseEntry(DateTime screenshotTime, string screenshotPath, ScreenshotParams screenshotParams) 
    { 
        ScreenshotTime   = screenshotTime;
        ScreenshotPath   = screenshotPath;
        ScreenshotParams = screenshotParams;
    }
}
