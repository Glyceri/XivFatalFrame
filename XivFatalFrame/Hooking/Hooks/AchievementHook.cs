using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal unsafe class AchievementHook : HookableElement
{
    private delegate void OnAchievementUnlockDelegate(Achievement* achievement, uint achievementID);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    private readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockingHook = null;

    public AchievementHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Dispose()
    {
        AchievementUnlockingHook?.Dispose();
    }

    public override void Init()
    {
        AchievementUnlockingHook?.Enable();
    }

    private bool IsAchievementCompleted(Achievement* achievement, uint achievementId)
    {
        if (achievement == null)
        {
            return true;
        }

        // This is a check the decomp code does
        if (achievementId > 0xEF0)
        {
            return true;
        }

        // Like the decomp code
        int byteIndex = (int)achievementId >> 3;
        int bitMask   = 1 << ((int)achievementId & 7);

        int achievementStatus = (achievement->CompletedAchievements[byteIndex] & bitMask);

        return achievementStatus != 0;
    }

    private void AchievementUnlockedDetour(Achievement* achievement, uint achievementId)
    {
        if (!IsAchievementCompleted(achievement, achievementId))
        {
            DalamudServices.PluginLog.Information($"Detected Acquired Achievement with ID: {achievementId}");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnAchievement, ScreenshotReason.Achievement);
        }

        AchievementUnlockingHook!.Original(achievement, achievementId);
    }

    public override void Reset()
        { }
}
