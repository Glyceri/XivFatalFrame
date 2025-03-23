using Lumina.Excel.Sheets;
using Lumina.Excel;
using System.Linq;

namespace XivFatalFrame;

internal class Sheets
{
    private readonly DalamudServices DalamudServices;

    private readonly ExcelSheet<ClassJob>       ClassJobs;
    private readonly ExcelSheet<Quest>          Quests;
    private readonly ExcelSheet<Item>           Items;

    public Quest[]  AllQuests   => Quests.ToArray();
    public Item[]   AllItems    => Items.ToArray();
    
    public Sheets(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;

        ClassJobs           = DalamudServices.DataManager.GetExcelSheet<ClassJob>();
        Quests              = DalamudServices.DataManager.GetExcelSheet<Quest>();
        Items               = DalamudServices.DataManager.GetExcelSheet<Item>();
    }

    public ClassJob? GetClassJob(uint id)
    {
        foreach (ClassJob classJob in ClassJobs)
        {
            if (classJob.RowId != id) continue;

            return classJob;
        }

        return null;
    }
}
