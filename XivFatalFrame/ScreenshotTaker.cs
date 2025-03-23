using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;

namespace XivFatalFrame;

internal unsafe class ScreenshotTaker : IDisposable
{
    private const int ScreenshotLogMessageId    = 1931;
    private const int ScreenshotKey             = 546;

    private bool TakeScreenshotPressed          = false;
    private bool SilenceLog                     = false;

    private readonly List<double> delays        = new List<double>();

    private delegate byte IsInputIdClickedDelegate(UIInputData* uiInputData, int key);
    private delegate void ShowLogMessageDelegate(RaptureLogModule* logModule, uint logMessageId);

    [Signature("E9 ?? ?? ?? ?? 83 7F ?? ?? 0F 8F ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8B CB", DetourName = nameof(IsInputIdClickedDetour))]
    private readonly Hook<IsInputIdClickedDelegate>? IsInputIdClickedHook = null;

    [Signature("E9 ?? ?? ?? ?? BA B3 11 00 00", DetourName = nameof(ShowLogMessageDetour))]
    private readonly Hook<ShowLogMessageDelegate>? ShowLogMessageHook = null;

    private readonly IPluginLog Log;
    private readonly Configuration Configuration;

    public ScreenshotTaker(DalamudServices dalamudServices, Configuration configuration)
    {
        Log = dalamudServices.PluginLog;
        Configuration = configuration;

        dalamudServices.Hooking.InitializeFromAttributes(this);
    }

    public void Init()
    {
        IsInputIdClickedHook?.Enable();
        ShowLogMessageHook?.Enable();
    }

    public void TakeScreenshot(double delay = 0)
    {
        if (IsInputIdClickedHook == null) return;
        if (!IsInputIdClickedHook.IsEnabled) return;

        Log.Verbose($"Taking screenshot in: {delay} seconds");

        delays.Add(delay);
    }

    public void Update(IFramework framework)
    {
        int delayCount = delays.Count;

        for (int i = delayCount - 1; i >= 0; i--)
        {
            delays[i] -= framework.UpdateDelta.TotalSeconds;

            if (delays[i] > 0) continue;

            delays.RemoveAt(i);

            TakeScreenshotPressed = true;
        }
    }

    private void ShowLogMessageDetour(RaptureLogModule* raptureLogModule, uint logMessageId)
    {
        if (logMessageId != ScreenshotLogMessageId)
        {
            return;
        }

        if (SilenceLog)
        {
            SilenceLog = false;
            return;
        }

        ShowLogMessageHook?.Original(raptureLogModule, logMessageId);
    }

    private byte IsInputIdClickedDetour(UIInputData* uiInputData, int key)
    {
        try
        {
            if (key == ScreenshotKey && TakeScreenshotPressed)
            {
                if (Configuration.SilenceLog)
                {
                    SilenceLog = true;
                }
            }

            byte outcome = IsInputIdClickedHook!.Original(uiInputData, key);

            if (key == ScreenshotKey && TakeScreenshotPressed)
            {
                TakeScreenshotPressed = false;
                outcome = 1;
            }

            return outcome;
        }
        catch(Exception e)
        {
            Log.Error(e, "IsInputIdClickedDetour");
        }

        return 0;
    }

    public void Dispose()
    {
        IsInputIdClickedHook?.Dispose();
        ShowLogMessageHook?.Dispose();
    }
}
