using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal class VistaUnlockHook : HookableElement
{
    private delegate nint VistaUnlockedDelegate(ushort index, int a2, int a3);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8B 4C 24 70 E8", DetourName = nameof(OnVistaUnlockedDetour))]
    private readonly Hook<VistaUnlockedDelegate>? VistaHook = null;

    public VistaUnlockHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Dispose()
    {
        VistaHook?.Dispose();
    }

    public override void Init()
    {
        VistaHook?.Enable();
    }

    private nint OnVistaUnlockedDetour(ushort index, int a2, int a3)
    {
        DalamudServices.PluginLog.Information($"Detected a vista unlocked at index: {index}");

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnEorzeaIncognita, ScreenshotReason.SightseeingLog);

        return VistaHook!.Original(index, a2, a3);
    }

    public override void Reset()
        { }
}
