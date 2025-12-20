using Dalamud.Hooking;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal unsafe class AchievementHook : HookableElement
{
    private delegate void OnAchievementUnlockDelegate(Achievement* achievement, uint achievementID);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    private readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockingHook = null;

    private readonly List<uint> SquareEnixSillyAchievements =
    [
        3811, // Some deep dungeon achievement that gets granted EVERY. SINGLE. LOGIN.
    ];

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
        if (achievementId > Sheets.AllAchievements.Length)
        {
            return true;
        }

        // Like the decomp code
        int byteIndex = (int)achievementId >> 3;
        int bitMask   = 1 << ((int)achievementId & 7);

        int achievementStatus = (achievement->CompletedAchievements[byteIndex] & bitMask);

        return achievementStatus != 0;
    }

    private bool IsAchievementSilly(Achievement* achievement, uint achievementId)
    {
        if (achievement == null)
        {
            return true;
        }

        if (!SquareEnixSillyAchievements.Contains(achievementId))
        {
            return false;
        }

        return true;
    }

    private void AchievementUnlockedDetour(Achievement* achievement, uint achievementId)
    {
        if (IsAchievementSilly(achievement, achievementId))
        {
            AchievementUnlockingHook!.Original(achievement, achievementId);

            return;
        }

        if (IsAchievementCompleted(achievement, achievementId))
        {
            AchievementUnlockingHook!.Original(achievement, achievementId);

            return;
        }

        DalamudServices.PluginLog.Information($"Detected Acquired Achievement with ID: [{achievementId}].{Environment.NewLine}This achievement has the name: [{Sheets.GetAchievement(achievementId)?.Name.ToDalamudString().TextValue ?? "No achievement name found."}].");

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnAchievement, ScreenshotReason.Achievement);
    }

    public override void Reset()
        { }
}
