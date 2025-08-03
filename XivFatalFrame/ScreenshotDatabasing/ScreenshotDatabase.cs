using System;
using System.IO;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using LiteDB;

namespace XivFatalFrame.ScreenshotDatabasing;

public class ScreenshotDatabase : IDisposable
{
    private const string DatabaseName = "fatalframe.db";
    private const string EntriesName  = "ScreenshotEntries";
    private const int    DatabaseStepSize = 5;

    private readonly LiteDatabase Database;
    private readonly ILiteCollection<DatabaseEntry> DatabaseEntries;
    private readonly IPluginLog Log;

    private double _timer       = 0;
    private int     _atIndex    = 0;

    public ScreenshotDatabase(IPluginLog log, IDalamudPluginInterface pluginInterface)
    {
        Log = log;

        Database = new LiteDatabase($"{pluginInterface.GetPluginConfigDirectory()}\\{DatabaseName}");

        Log.Verbose("Created database at: " + $"{pluginInterface.GetPluginConfigDirectory()}\\{DatabaseName}");

        DatabaseEntries = Database.GetCollection<DatabaseEntry>(EntriesName);

        _ = DatabaseEntries.EnsureIndex(e => e.ScreenshotTime);
    }

    public void AddEntryToDatabase(DatabaseEntry entry)
    {
        Log.Verbose($"Added database entry: {entry}");

        _ = DatabaseEntries.Insert(entry);
    }

    public void Update(IFramework framework)
    {
        _timer += framework.UpdateDelta.TotalSeconds;

        if (_timer <= 5)
        {
            return;
        }

        _timer = 0;

        DatabaseEntry[] entries = DatabaseEntries.Find(Query.All(), _atIndex, DatabaseStepSize).ToArray();

        _atIndex += DatabaseStepSize;

        if (_atIndex > DatabaseEntries.Count())
        {
            _atIndex = 0;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            DatabaseEntry entry = entries[i];

            try
            {
                if (!File.Exists(entry.ScreenshotPath))
                {
                    _ = DatabaseEntries.Delete(entry.Id);

                    Log.Error(entry.ScreenshotPath + "was not found and thus its entry has been removed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Path not found.");
            }
        }
    }

    public DatabaseEntry[] GetEntries()
    {
        return DatabaseEntries.FindAll().ToArray();
    }

    public void Dispose()
    {
        Database.Dispose();
    }
}