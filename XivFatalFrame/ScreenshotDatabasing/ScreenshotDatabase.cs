using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using LiteDB;
using System;

namespace XivFatalFrame.ScreenshotDatabasing;

public unsafe class ScreenshotDatabase : IDisposable
{
    private const string DatabaseName       = "fatalframe.db";
    private const string UserDatabaseName   = "Users";
    private const int    DatabaseStepSize   = 5;

    private readonly LiteDatabase Database;
    private readonly ILiteCollection<DatabaseUser> DatabaseEntries;
    private readonly IPluginLog Log;

    private DatabaseUser? ActiveUser;

    private double  _timer   = 0;
    private int     _atIndex = 0;

    public ScreenshotDatabase(IPluginLog log, IDalamudPluginInterface pluginInterface)
    {
        Log = log;

        Database = new LiteDatabase($"{pluginInterface.GetPluginConfigDirectory()}\\{DatabaseName}");

        Log.Verbose("Created database at: " + $"{pluginInterface.GetPluginConfigDirectory()}\\{DatabaseName}");

        DatabaseEntries = Database.GetCollection<DatabaseUser>(UserDatabaseName);
    }

    public void AddEntryToDatabase(DatabaseEntry entry)
    {
        if (ActiveUser == null)
        {
            return;
        }

        Log.Verbose($"Added database entry: {entry}");

        ActiveUser.Entries.Add(entry);

        _ = DatabaseEntries.Update(ActiveUser);
    }

    public void SetActiveUser(IPlayerCharacter? player)
    {
        Log.Verbose($"Database: Set active user: [{player}]");

        ActiveUser = null;

        if (player == null) 
        {
            return;
        }

        BattleChara* playerBChara = (BattleChara*)player.Address;
        if (playerBChara == null)
        {
            return;
        }

        ulong contentId = playerBChara->ContentId;
        if (contentId == 0) 
        {
            return;
        }

        ushort homeWorld = playerBChara->HomeWorld;
        if (homeWorld == 0) 
        {
            return;
        }

        string userName = playerBChara->NameString;
        if (userName.IsNullOrWhitespace())
        {
            return;
        }

        foreach (DatabaseUser user in DatabaseEntries.FindAll())
        {
            if (user.ContentId != contentId)
            {
                continue;
            }

            ActiveUser = user;

            ActiveUser.Name         = userName;
            ActiveUser.Homeworld    = homeWorld;

            _ = DatabaseEntries.Update(ActiveUser);

            return;
        }

        ActiveUser = new DatabaseUser(contentId, userName, homeWorld);

        _ = DatabaseEntries.Insert(ActiveUser);
    }

    public void Update(IFramework framework)
    {
        _timer += framework.UpdateDelta.TotalSeconds;

        if (_timer <= 5)
        {
            return;
        }

        _timer = 0;

        // TODO: Reïmplement here database entry deletion when image no longer exists.
    }

    public void Dispose()
    {
        Database.Dispose();
    }
}