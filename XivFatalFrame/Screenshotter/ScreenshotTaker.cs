using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Numerics;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.ScreenshotDatabasing;
using XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;
using XivFatalFrame.Services;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

namespace XivFatalFrame.Screenshotter;

internal unsafe class ScreenshotTaker : IDisposable
{
    private const int ScreenshotLogMessageId    = 1931;
    private const int ScreenshotKey             = 551;

    private bool TakeScreenshotPressed          = false;
    private bool OurLog                         = false;
    private bool OurScreenshot                  = false;
    private bool OurChat                        = false;
    private bool MessageIsScreenshotLog         = false;
    private bool IsScreenshotScheduled          = false;

    private readonly List<ScreenshotElement> delays = new List<ScreenshotElement>();

    private delegate byte IsInputIdClickedDelegate(UIInputData* uiInputData, int key);
    private delegate void ShowLogMessageDelegate(RaptureLogModule* logModule, uint logMessageId);
    private delegate nint ScreenShot_CallbackDelegate(nint a1, int a2);
    private delegate byte ScreenShot_ScheduleScreenshotDelegate(nint a1, nint callback, nint callbackParam);
    private delegate int Screenshot_CreateFilePathFromUtf8String(nint a1, Utf8String* path, char a3);

    [Signature("E9 ?? ?? ?? ?? 83 7F ?? ?? 0F 8F ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8B CB", DetourName = nameof(IsInputIdClickedDetour))]
    private readonly Hook<IsInputIdClickedDelegate>? IsInputIdClickedHook = null;

    [Signature("E9 ?? ?? ?? ?? BA B3 11 00 00", DetourName = nameof(ShowLogMessageDetour))]
    private readonly Hook<ShowLogMessageDelegate>? ShowLogMessageHook = null;

    [Signature("48 89 5C 24 08 57 48 83 EC 20 BB 8B 07 00 00", DetourName = nameof(ScreenShotCallbackDetour))]
    private readonly Hook<ScreenShot_CallbackDelegate>? ScreenShotCallbackHook = null;

    [Signature("E8 ?? ?? ?? ?? 84 C0 75 15 C6 05 ?? ?? ?? ?? ?? ", DetourName = nameof(ScreenShotScheduleDetour))]
    private readonly Hook<ScreenShot_ScheduleScreenshotDelegate>? ScreenShotScheduleHook = null;

    [Signature("E8 ?? ?? ?? ?? 48 8D 4C 24 20 E8 ?? ?? ?? ?? 48 8D 8C 24 90 00 00 00 E8 ?? ?? ?? ?? 48 8B C6", DetourName = nameof(CreateFilePathFromUtf8StringDetour))]
    private readonly Hook<Screenshot_CreateFilePathFromUtf8String>? CreateFilePathFromUtf8StringHook = null;

    private readonly DalamudServices    DalamudServices;
    private readonly IPluginLog         Log;
    private readonly Configuration      Configuration;
    private readonly IPVPReader         PVPReader;
    private readonly ScreenshotDatabase ScreenshotDatabase;

    private          ScreenshotParams?       lastReason  = null;
    private          string                  lastPath    = string.Empty;
    private readonly List<ScreenshotParams?> paramStack  = new List<ScreenshotParams?>();
    private readonly List<string>            pathStack   = new List<string>();

    public ScreenshotTaker(DalamudServices dalamudServices, Configuration configuration, ScreenshotDatabase screenshotDatabase, IPVPReader pvpReader)
    {
        DalamudServices     = dalamudServices;
        Log                 = dalamudServices.PluginLog;
        Configuration       = configuration;
        PVPReader           = pvpReader;
        ScreenshotDatabase  = screenshotDatabase;

        dalamudServices.Hooking.InitializeFromAttributes(this);
    }

    public void Init()
    {
        IsInputIdClickedHook?.Enable();
        ShowLogMessageHook?.Enable();
        ScreenShotCallbackHook?.Enable();
        ScreenShotScheduleHook?.Enable();
        CreateFilePathFromUtf8StringHook?.Enable();

        DalamudServices.ChatGui.CheckMessageHandled += OnChatMessage;
    }

    public void TakeScreenshot(SerializableSetting setting, ScreenshotParams screenshotParams)
    {
        Log.Verbose($"Take a screenshot with the params: {screenshotParams.GetType()} [{screenshotParams.ToString()}]");

        if (setting is SerializablePvpSetting pvpSetting && PVPReader.IsInPVP)
        {
            HandleAsPVP(pvpSetting, screenshotParams);
        }
        else
        {
            HandleAsPVE(setting, screenshotParams);
        }
    }

    private void HandleAsPVP(SerializablePvpSetting pvpSetting, ScreenshotParams screenshotParams)
    {
        if (!pvpSetting.EnabledInPvp)
        {
            Log.Verbose($"Want's to take a screenshot but the config disallowed it in a pvp setting.");

            return;
        }

        TakeScreenshot(pvpSetting.AfterDelayPVP, screenshotParams);
    }

    private void HandleAsPVE(SerializableSetting setting, ScreenshotParams screenshotParams)
    {
        if (!setting.TakeScreenshot)
        {
            Log.Verbose($"Want's to take a screenshot but the config disallowed it.");

            return;
        }

        TakeScreenshot(setting.AfterDelay, screenshotParams);
    }

    private void TakeScreenshot(double delay, ScreenshotParams screenshotParams)
    {
        if (IsInputIdClickedHook == null) return;
        if (!IsInputIdClickedHook.IsEnabled) return;

        Log.Verbose($"Taking screenshot in: {delay} seconds, {screenshotParams}");

        AddScreenshotToQueue(delay, screenshotParams);
    }

    private void AddScreenshotToQueue(double delay, ScreenshotParams screenshotParams)
    {
        // No way should there ever be queued more than 5 at once, this is just a safety precaution in case I pull a Glyceri moment later.
        if (delays.Count > 5)
        {
            Log.Verbose($"Screenshot got waived due to max cap being reached.");

            return;
        }

        ScreenshotElement screenshotElement = new ScreenshotElement(delay, screenshotParams);

        delays.Add(screenshotElement);
    }

    public void Update(IFramework framework)
    {
        int delayCount = delays.Count;

        for (int i = delayCount - 1; i >= 0; i--)
        {
            delays[i].Timer -= framework.UpdateDelta.TotalSeconds;

            if (delays[i].Timer > 0) continue;

            lastReason = delays[i].Params;

            delays.RemoveAt(i);

            TakeScreenshotPressed = true;
        }
    }

    private byte ScreenShotScheduleDetour(nint a1, nint callback, nint callbackParam)
    {
#if DEBUG
        Log.Verbose("Scheduled Screenshot!");
#endif

        IsScreenshotScheduled = true;

        return ScreenShotScheduleHook!.Original(a1, callback, callbackParam);
    }
    
    private int CreateFilePathFromUtf8StringDetour(nint a1, Utf8String* path, char a3)
    {
        if (IsScreenshotScheduled)
        {
            if (path != null)
            {
                string currentPath = path->ToString();

#if DEBUG
                Log.Verbose($"Created file path at: {currentPath}");
#endif

                lastPath = currentPath;
            }
        }

        return CreateFilePathFromUtf8StringHook!.Original(a1, path, a3);
    }

    private void ShowLogMessageDetour(RaptureLogModule* raptureLogModule, uint logMessageId)
    {
#if DEBUG
        Log.Verbose($"Log Message: {logMessageId}");
#endif

        if (logMessageId == ScreenshotLogMessageId)
        {
            MessageIsScreenshotLog = true;

            if (OurLog)
            {
                OurChat = true;
            }
        }

        ShowLogMessageHook?.Original(raptureLogModule, logMessageId);
    }

    private nint ScreenShotCallbackDetour(nint a1, int a2)
    {
#if DEBUG
        Log.Verbose($"Screenshot callback!");
#endif

        if (IsScreenshotScheduled)
        {
            if (pathStack.Count > 10)
            {
                Log.Error("pathStack somehow got over 10, please tell Glyceri about this.");

                pathStack.Clear();
            }

            pathStack.Add(lastPath);
        }    

        IsScreenshotScheduled = false;

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
            bool isScreenshotKey = (key == ScreenshotKey);

            if (isScreenshotKey && TakeScreenshotPressed)
            {
                OurScreenshot = true;
            }

            byte outcome = IsInputIdClickedHook!.Original(uiInputData, key);

            // Screenshot has been pressed
            if (isScreenshotKey && (outcome != 0 || TakeScreenshotPressed))
            {
                if (TakeScreenshotPressed)  // Custom Reason
                {
                    if (paramStack.Count > 10)
                    {
                        paramStack.Clear();

                        Log.Error("reasonStack exceeded 10, please tell Glyceri if you ever see this message.");
                    }

                    if (lastReason != null)
                    {
                        paramStack.Add(lastReason);
                    }
                    else
                    {
                        paramStack.Add(null);
                    }

                    lastReason = null;
                }
                else
                {
                    paramStack.Add(null);
                }
            }    

            if (isScreenshotKey && TakeScreenshotPressed)
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
        ScreenshotParams? currentParams  = null;
        string            currentPath    = string.Empty;

        if (MessageIsScreenshotLog)
        {
            if (paramStack.Count > 0)
            {
                currentParams = paramStack[0];

                paramStack.RemoveAt(0);
            }

            if (pathStack.Count > 0)
            {
                currentPath = pathStack[0];

                pathStack.RemoveAt(0);
            }

#if DEBUG
            Log.Verbose($"Screenshot reason: [{currentParams?.ToString() ?? "currentParams is NULL!"}]");
            Log.Verbose($"Screenshot made at: [{currentPath}]");
#endif
        }

        MessageIsScreenshotLog = false;

        if (!OurChat)
        {
            return;
        }

        OurChat = false;

        if (currentParams == null)
        {
            return;
        }

        if (currentPath.IsNullOrWhitespace())
        {
            return;
        }

        ScreenshotDatabase.AddEntryToDatabase(new DatabaseEntry(DateTime.Now, currentPath, currentParams));

        if (handled)
        {
            return;
        }

        if (!Configuration.CustomLogMessage)
        {
            return;
        }

        if (Configuration.SilenceLog)
        {
            handled = true;

            return;
        }

        LSeStringBuilder builder = new LSeStringBuilder();

        _ = builder.PushColorRgba(new Vector4(1.0f, 0.4f, 0.4f, 1f));
        _ = builder.Append($"Fatal Frame took a Screenshot. [{currentParams.ScreenshotReason}]");
        _ = builder.PopColor();

        message.Payloads.Clear();
        message.Payloads.AddRange(builder.ToReadOnlySeString().ToDalamudString().Payloads);
    }

    public void Dispose()
    {
        DalamudServices.ChatGui.ChatMessage -= OnChatMessage;

        IsInputIdClickedHook?.Dispose();
        ShowLogMessageHook?.Dispose();
        ScreenShotCallbackHook?.Dispose();
        ScreenShotScheduleHook?.Dispose();
        CreateFilePathFromUtf8StringHook?.Dispose();
    }
}
