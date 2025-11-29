using FFXIVClientStructs.FFXIV.Client.Game.UI;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

using LuminaClass = Lumina.Excel.Sheets.ClassJob;

namespace XivFatalFrame.Hooking.Hooks;

// Im pretty sure this is bugged, but then I can never replicate it ... sigh
internal unsafe class LevelChangedHook : HookableElement
{
    private short[] _currentClassJobLevels = [];

    public LevelChangedHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Dispose()
    {
        DalamudServices.ClientState.LevelChanged -= OnLevelChanged;
    }

    public override void Init()
    {
        DalamudServices.ClientState.LevelChanged += OnLevelChanged;
    }

    public override void Reset()
    {
        _currentClassJobLevels = PlayerState.Instance()->ClassJobLevels.ToArray();
    }

    private void OnLevelChanged(uint classJobId, uint level)
    {
        DalamudServices.PluginLog.Verbose($"Detected a level change on the job: {classJobId} to level: {level}");

        LuminaClass? classJob = Sheets.GetClassJob(classJobId);

        if (classJob == null)
        {
            DalamudServices.PluginLog.Information("Couldn't find the classjob in the sheets???? HOW");

            return;
        }

        sbyte arrayIndex = classJob.Value.ExpArrayIndex;

        if (arrayIndex < 0 || arrayIndex >= _currentClassJobLevels.Length)
        {
            DalamudServices.PluginLog.Information($"Array index is out of range: {arrayIndex} on the classJobArray: {_currentClassJobLevels.Length}");

            return;
        }

        short currentLevel = _currentClassJobLevels[arrayIndex];

        if (currentLevel >= level)
        {
            DalamudServices.PluginLog.Information($"This resulted in no actual change.");

            return;
        }

        RequestRefresh();

        DalamudServices.PluginLog.Information($"The class: {classJobId}, {arrayIndex}, {classJob.Value.Name.ExtractText()} leveled up from: {currentLevel} to {level}. This has been marked.");

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnLevelup, ScreenshotReason.LevelUp);
    }
}
