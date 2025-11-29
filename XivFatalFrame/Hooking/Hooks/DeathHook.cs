using Dalamud.Plugin.Services;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal class DeathHook : HookableElement
{
    private bool IsDead = false;

    public DeathHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Update(IFramework framework)
    {
        if (DalamudServices.ObjectTable.LocalPlayer == null)
        {
            IsDead = true;

            return;
        }

        if (!DalamudServices.ObjectTable.LocalPlayer.IsValid())
        {
            IsDead = true;

            return;
        }

        if (!DalamudServices.ObjectTable.LocalPlayer.IsDead)
        {
            IsDead = false;
        }

        if (IsDead)
        {
            return;
        }

        if (!DalamudServices.ObjectTable.LocalPlayer.IsDead)
        {
            return;
        }

        IsDead = true;

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnDeath, ScreenshotReason.Death);
    }

    public override void Dispose()
        { }

    public override void Init()
        { }

    public override void Reset()
        { }
}
