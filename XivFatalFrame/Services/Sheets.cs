using Lumina.Excel.Sheets;
using Lumina.Excel;

namespace XivFatalFrame.Services;

internal class Sheets
{
    private readonly DalamudServices DalamudServices;

    private readonly ExcelSheet<ClassJob>    ClassJobs;
    private readonly ExcelSheet<Quest>       Quests;
    private readonly ExcelSheet<Item>        Items;
    private readonly ExcelSheet<Achievement> Achievements;
    
    public Sheets(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;

        ClassJobs    = DalamudServices.DataManager.GetExcelSheet<ClassJob>();
        Quests       = DalamudServices.DataManager.GetExcelSheet<Quest>();
        Items        = DalamudServices.DataManager.GetExcelSheet<Item>();
        Achievements = DalamudServices.DataManager.GetExcelSheet<Achievement>();
    }

    public Quest[] AllQuests 
        => [.. Quests];

    public Item[] AllItems
        => [.. Items];

    public Achievement[] AllAchievements
        => [.. Achievements];

    public Achievement? GetAchievement(uint achievementId)
    {
        if (!Achievements.TryGetRow(achievementId, out Achievement achievement))
        {
            return null;
        }

        return achievement;
    }

    public ClassJob? GetClassJob(uint id)
    {
        foreach (ClassJob classJob in ClassJobs)
        {
            if (classJob.RowId != id)
            {
                continue;
            }

            return classJob;
        }

        return null;
    }
}
