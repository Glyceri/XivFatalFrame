using Lumina.Excel.Sheets;
using Lumina.Excel;

namespace XivFatalFrame.Services;

internal class Sheets
{
    private readonly DalamudServices DalamudServices;

    private readonly ExcelSheet<ClassJob> ClassJobs;
    private readonly ExcelSheet<Quest>    Quests;
    private readonly ExcelSheet<Item>     Items;
    
    public Sheets(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;

        ClassJobs = DalamudServices.DataManager.GetExcelSheet<ClassJob>();
        Quests    = DalamudServices.DataManager.GetExcelSheet<Quest>();
        Items     = DalamudServices.DataManager.GetExcelSheet<Item>();
    }

    public Quest[] AllQuests 
        => [.. Quests];

    public Item[] AllItems
        => [.. Items];

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
