using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkUIColorHolder.Delegates;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

namespace XivFatalFrame;

internal unsafe class ScreenshotTaker : IDisposable
{
    private const int ScreenshotLogMessageId    = 1931;
    private const int ScreenshotKey             = 546;

    private bool TakeScreenshotPressed          = false;
    private bool OurLog                         = false;
    private bool OurScreenshot                  = false;
    private bool OurChat                        = false;

    private readonly List<double> delays        = new List<double>();

    private delegate byte IsInputIdClickedDelegate(UIInputData* uiInputData, int key);
    private delegate void ShowLogMessageDelegate(RaptureLogModule* logModule, uint logMessageId);
    private delegate nint ScreenShotCallbackDelegate(nint a1, int a2);

    [Signature("E9 ?? ?? ?? ?? 83 7F ?? ?? 0F 8F ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8B CB", DetourName = nameof(IsInputIdClickedDetour))]
    private readonly Hook<IsInputIdClickedDelegate>? IsInputIdClickedHook = null;

    [Signature("E9 ?? ?? ?? ?? BA B3 11 00 00", DetourName = nameof(ShowLogMessageDetour))]
    private readonly Hook<ShowLogMessageDelegate>? ShowLogMessageHook = null;

    [Signature("48 89 5C 24 08 57 48 83 EC 20 BB 8B 07 00 00", DetourName = nameof(ScreenShotCallbackDetour))]
    private readonly Hook<ScreenShotCallbackDelegate>? ScreenShotCallbackHook = null;

    private readonly DalamudServices DalamudServices;
    private readonly IPluginLog Log;
    private readonly Configuration Configuration;

    public ScreenshotTaker(DalamudServices dalamudServices, Configuration configuration)
    {
        DalamudServices = dalamudServices;
        Log = dalamudServices.PluginLog;
        Configuration = configuration;

        dalamudServices.Hooking.InitializeFromAttributes(this);
    }

    public void Init()
    {
        IsInputIdClickedHook?.Enable();
        ShowLogMessageHook?.Enable();
        ScreenShotCallbackHook?.Enable();

        DalamudServices.ChatGui.CheckMessageHandled += OnChatMessage;
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

        if (OurLog)
        {
            if (Configuration.SilenceLog)
            {
                return;
            }
        }

        OurChat = true;

        ShowLogMessageHook?.Original(raptureLogModule, logMessageId);
    }

    private nint ScreenShotCallbackDetour(nint a1, int a2)
    {
        if (OurScreenshot)
        {
            OurLog = true;
        }

        nint outcome = ScreenShotCallbackHook!.Original(a1, a2);

        if (OurLog)
        {
            OurLog = false;
        }

        OurScreenshot = false;

        return outcome;
    }

    private byte IsInputIdClickedDetour(UIInputData* uiInputData, int key)
    {
        try
        {
            if (key == ScreenshotKey && TakeScreenshotPressed)
            {
                OurScreenshot = true;
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

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool handled)
    {
        if (!OurChat)
        {
            return;
        }

        OurChat = false;

        if (!Configuration.CustomLogMessage)
        {
            return;
        }

        LSeStringBuilder builder = new LSeStringBuilder();

        builder.PushColorRgba(new Vector4(1.0f, 0.4f, 0.4f, 1f));
        builder.Append("Fatal Frame took a Screenshot.");
        builder.PopColor();

        message.Payloads.Clear();
        message.Payloads.AddRange(builder.ToReadOnlySeString().ToDalamudString().Payloads);
    }

    public void Dispose()
    {
        DalamudServices.ChatGui.ChatMessage -= OnChatMessage;

        IsInputIdClickedHook?.Dispose();
        ShowLogMessageHook?.Dispose();
        ScreenShotCallbackHook?.Dispose();
    }
}
