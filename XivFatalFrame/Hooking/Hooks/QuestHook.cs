using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Collections.Generic;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

using LuminaQuest = Lumina.Excel.Sheets.Quest;

namespace XivFatalFrame.Hooking.Hooks;

internal unsafe class QuestHook : HookableElement
{
    private readonly List<uint> _questsCompleted = [];

    private byte _lastAcceptedQuestCount = 0;

    public QuestHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Update(IFramework framework)
    {
        if (DalamudServices.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        byte numAcceptedQuests = QuestManager.Instance()->NumAcceptedQuests;

        if (numAcceptedQuests == _lastAcceptedQuestCount)
        {
            return;
        }

        _lastAcceptedQuestCount = numAcceptedQuests;

        foreach (LuminaQuest quest in Sheets.AllQuests)
        {
            uint questRowID = quest.RowId;

            if (!QuestManager.IsQuestComplete(questRowID))
            {
                continue;
            }

            if (_questsCompleted.Contains(questRowID))
            {
                continue;
            }

            _questsCompleted.Add(questRowID);

            DalamudServices.PluginLog.Information($"Quest with ID {questRowID} and name {quest.Name.ExtractText()} has been found.");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnQuestComplete, ScreenshotReason.QuestCompletion);
        }
    }

    public override void Reset()
    {
        _lastAcceptedQuestCount = QuestManager.Instance()->NumAcceptedQuests;

        _questsCompleted.Clear();

        foreach (LuminaQuest quest in Sheets.AllQuests)
        {
            if (!QuestManager.IsQuestComplete(quest.RowId))
            {
                continue;
            }

            _questsCompleted.Add(quest.RowId);
        }
    }

    public override void Dispose()
        { }

    public override void Init()
        { }
}
