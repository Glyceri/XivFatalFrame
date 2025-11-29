using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal class DutyCompleteHook : HookableElement
{
    public DutyCompleteHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Dispose()
    {
        DalamudServices.DutyState.DutyCompleted -= OnDutyCompleted;
    }

    public override void Init()
    {
        DalamudServices.DutyState.DutyCompleted += OnDutyCompleted;
    }

    private void OnDutyCompleted(object? _, ushort dutyID)
    {
        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnDutyCompletion, ScreenshotReason.DutyCompletion);
    }

    public override void Reset()
        { }
}
