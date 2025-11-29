using Dalamud.Plugin.Services;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal class PVPHook : HookableElement
{
    public PVPHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Update(IFramework framework)
    {
        PVPSetter.SetPVPState(false);

        if (DalamudServices.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        if (!DalamudServices.ObjectTable.LocalPlayer.IsValid())
        {
            return;
        }

        PVPSetter.SetPVPState(DalamudServices.ClientState.IsPvP);
    }

    public override void Dispose() 
        { }

    public override void Init() 
        { }

    public override void Reset()
        { }
}
