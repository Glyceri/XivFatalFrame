using LiteDB;
using System;
using System.Collections.Generic;

namespace XivFatalFrame.ScreenshotDatabasing;

[Serializable]
public class DatabaseUser
{
    [BsonId]
    public ObjectId Id          { get; set; } = ObjectId.NewObjectId();

    public ulong    ContentId   { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public ushort   Homeworld   { get; set; }

    public List<DatabaseEntry> Entries { get; set; } = new List<DatabaseEntry>();

    public DatabaseUser() { }

    public DatabaseUser(ulong contentId, string name, ushort homeworld)
    {
        ContentId   = contentId;
        Name        = name;
        Homeworld   = homeworld;
    }

    public DatabaseUser(ulong contentId, string name, ushort homeworld, List<DatabaseEntry> entries) : this(contentId, name, homeworld)
    {
        Entries = entries;
    }
}
