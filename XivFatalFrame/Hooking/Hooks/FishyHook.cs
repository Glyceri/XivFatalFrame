using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Numerics;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

namespace XivFatalFrame.Hooking.Hooks;

internal unsafe class FishyHook : HookableElement
{
    private const uint SpearFishIdOffset = 20000;

    private byte[] _fishStore      = [];
    private byte[] _spearFishStore = [];

    public FishyHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter)
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter) { }

    public override void Update(IFramework framework)
    {
        if (DalamudServices.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        uint? fishOutcome = CheckFishies(ref _fishStore, PlayerState.Instance()->CaughtFish);

        if (fishOutcome != null)
        {
            DalamudServices.PluginLog.Information($"Found new fish caught with ID: {fishOutcome.Value}");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnFishCaught, ScreenshotReason.Fish);
        }

        uint? spfishOutcome = CheckFishies(ref _spearFishStore, PlayerState.Instance()->CaughtSpearfish);

        if (spfishOutcome != null)
        {
            spfishOutcome += SpearFishIdOffset;
            DalamudServices.PluginLog.Information($"Found new spearfish caught with ID: {spfishOutcome.Value}");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnFishCaught, ScreenshotReason.Fish);
        }
    }

    private uint? GetCaughtFishIndices(Span<byte> oldStore, Span<byte> newStore)
    {
        // Calculate the minimum length of both bitmasks
        int maxLength = Math.Min(oldStore.Length, newStore.Length);

        // Iterate through each byte in the bitmask arrays
        for (int byteIndex = 0; byteIndex < maxLength; byteIndex++)
        {
            // Get the difference between new and old byte (new byte with old byte masked out)
            byte difference = (byte)(newStore[byteIndex] & ~oldStore[byteIndex]);

            // If there is any difference, find the fish index
            if (difference != 0)
            {
                // Use a fast bit scan to find the first bit set in 'difference'
                int bitIndex = BitOperations.TrailingZeroCount(difference);

                // Return the global fish index by combining byte and bit indices
                return (uint)(byteIndex * 8 + bitIndex);
            }
        }

        return null;
    }

    private uint? CheckFishies(ref byte[] store, Span<byte> bitmask)
    {
        Span<byte> span = bitmask;

        bool fishyEquals = new Span<byte>(store, 0, store.Length).SequenceEqual(span);

        if (fishyEquals)
        {
            return null;
        }

        uint? outcome = GetCaughtFishIndices(store, span);

        store = span.ToArray();

        return outcome;
    }

    public override void Reset()
    {
        _fishStore      = PlayerState.Instance()->CaughtFish.ToArray();
        _spearFishStore = PlayerState.Instance()->CaughtSpearfish.ToArray();
    }

    public override void Dispose() 
        { }

    public override void Init() 
        { }
}
