using Dalamud.Plugin.Services;
using System;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking;

internal abstract class HookableElement : IDisposable
{
    protected readonly HookHandler     HookHandler;
    protected readonly DalamudServices DalamudServices;
    protected readonly ScreenshotTaker ScreenshotTaker;
    protected readonly Configuration   Configuration;
    protected readonly Sheets          Sheets;
    protected readonly IPVPSetter      PVPSetter;

    public HookableElement(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter)
    {
        HookHandler     = hookHandler;
        DalamudServices = dalamudServices;
        ScreenshotTaker = screenshotTaker;
        Configuration   = configuration;
        Sheets          = sheets;
        PVPSetter       = pvpSetter;

        DalamudServices.Hooking.InitializeFromAttributes(this);
    }

    protected void RequestRefresh()
    {
        HookHandler.Reset();
    }

    public virtual void Update(IFramework framework)
        { }

    public abstract void Init();
    public abstract void Reset();
    public abstract void Dispose();
}
