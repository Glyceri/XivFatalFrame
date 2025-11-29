using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using XivFatalFrame.Hooking.Hooks;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking;

internal class HookHandler : IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly ScreenshotTaker ScreenshotTaker;
    private readonly Configuration   Configuration;
    private readonly Sheets          Sheets;
    private readonly IPVPSetter      PVPSetter;

    private readonly List<HookableElement> HookableElements = [];

    public HookHandler(DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter)
    {
        DalamudServices = dalamudServices;
        ScreenshotTaker = screenshotTaker;
        Configuration   = configuration;
        Sheets          = sheets;
        PVPSetter       = pvpSetter;

        _Register();
        Init();
        Reset();
    }

    private void _Register()
    {
        Register(new AchievementHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new DeathHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new PVPHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new VistaUnlockHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new DutyCompleteHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new ItemUnlockHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new QuestHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new LevelChangedHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new LoginHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
        Register(new FishyHook(this, DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPSetter));
    }

    private void Register(HookableElement hookableElement)
    {
        _ = HookableElements.Remove(hookableElement);

        HookableElements.Add(hookableElement);
    }

    private void Init()
    {
        foreach (HookableElement hookableElement in HookableElements)
        {
            hookableElement.Init();
        }
    }

    public void Update(IFramework framework)
    {
        foreach (HookableElement hookableElement in HookableElements)
        {
            hookableElement.Update(framework);
        }
    }
    
    public void Reset()
    {
        foreach (HookableElement hookableElement in HookableElements)
        {
            hookableElement.Reset();
        }
    }

    public void Dispose()
    {
        foreach (HookableElement hookableElement in HookableElements)
        {
            try
            {
                hookableElement.Dispose();
            }
            catch (Exception ex)
            {
                DalamudServices.PluginLog.Error(ex.Message);
            }
        }

        HookableElements.Clear();
    }
}
