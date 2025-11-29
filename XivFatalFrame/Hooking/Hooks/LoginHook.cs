using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal class LoginHook : HookableElement
{
    public LoginHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Dispose()
    {
        DalamudServices.ClientState.Login -= OnLogin;
    }

    public override void Init()
    {
        DalamudServices.ClientState.Login += OnLogin;
    }

    private void OnLogin()
    {
        DalamudServices.PluginLog.Verbose($"Detected a login, Reset() has been called.");

        RequestRefresh();
    }

    public override void Reset()
        { }
}
