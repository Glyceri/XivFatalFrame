using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;

using LuminaItem             = Lumina.Excel.Sheets.Item;
using LuminaTrippleTriadCard = Lumina.Excel.Sheets.TripleTriadCard;
using LuminaOrchestrion      = Lumina.Excel.Sheets.Orchestrion;

namespace XivFatalFrame.Hooking.Hooks;

// This will eventually become hasels unlock service c:
internal unsafe class ItemUnlockHook : HookableElement
{
    private delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float deltaTime);

    private readonly Hook<RaptureAtkModuleUpdateDelegate>? RaptureAtkModuleUpdateHook;

    private readonly List<uint> _unlockedItems = [];

    public ItemUnlockHook(HookHandler hookHandler, DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter) 
        : base(hookHandler, dalamudServices, screenshotTaker, configuration, sheets, pvpSetter)
    {
        RaptureAtkModuleUpdateHook = DalamudServices.Hooking.HookFromAddress<RaptureAtkModuleUpdateDelegate>((nint)RaptureAtkModule.StaticVirtualTablePointer->Update, RaptureAtkModule_UpdateDetour);
    }

    public override void Dispose()
    {
        RaptureAtkModuleUpdateHook?.Dispose();
    }

    public override void Init()
    {
        RaptureAtkModuleUpdateHook?.Enable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetCompanionID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetBuddyEquipID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetMountID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetSecretRecipeID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetUnlockLinkID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetFolkloreID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetOrnamentID(LuminaItem item)
        => GetItemActionID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetGlassesID(LuminaItem item)
        => GetItemAdditionalDataID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetTrippleTriadID(LuminaItem item)
        => GetItemAdditionalDataID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetOrchestrionID(LuminaItem item)
        => GetItemAdditionalDataID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetFramerKitID(LuminaItem item)
        => GetItemAdditionalDataID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetItemActionID(LuminaItem item)
        => item.ItemAction.Value.Data[0];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetItemAdditionalDataID(LuminaItem item)
        => item.AdditionalData.RowId;

    private bool IsUnlocked(LuminaItem item, out bool itemIsUnlocked)
    {
        itemIsUnlocked = false;

        if (item.ItemAction.RowId == 0)
        {
            return false;
        }

        switch ((ItemActionType)item.ItemAction.Value.Type)
        {
            case ItemActionType.Companion:
            {
                itemIsUnlocked = UIState.Instance()->IsCompanionUnlocked(GetCompanionID(item));

                return true;
            }

            case ItemActionType.BuddyEquip:
            {
                itemIsUnlocked = UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(GetBuddyEquipID(item));

                return true;
            }

            case ItemActionType.Mount:
            {
                itemIsUnlocked = PlayerState.Instance()->IsMountUnlocked(GetMountID(item));

                return true;
            }

            case ItemActionType.SecretRecipeBook:
            {
                itemIsUnlocked = PlayerState.Instance()->IsSecretRecipeBookUnlocked(GetSecretRecipeID(item));

                return true;
            }

            case ItemActionType.UnlockLink:
            {
                itemIsUnlocked = UIState.Instance()->IsUnlockLinkUnlocked(GetUnlockLinkID(item));

                return true;
            }

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<LuminaTrippleTriadCard>():
            {
                itemIsUnlocked = UIState.Instance()->IsTripleTriadCardUnlocked((ushort)GetTrippleTriadID(item));

                return true;
            }

            case ItemActionType.FolkloreTome:
            {
                itemIsUnlocked = PlayerState.Instance()->IsFolkloreBookUnlocked(GetFolkloreID(item));

                return true;
            }

            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Lumina.Excel.Sheets.Orchestrion>():
            {
                itemIsUnlocked = PlayerState.Instance()->IsOrchestrionRollUnlocked(GetOrchestrionID(item));

                return true;
            }

            case ItemActionType.FramersKit:
            {
                itemIsUnlocked = PlayerState.Instance()->IsFramersKitUnlocked(GetFramerKitID(item));

                return true;
            }

            case ItemActionType.Ornament:
            {
                itemIsUnlocked = PlayerState.Instance()->IsOrnamentUnlocked(GetOrnamentID(item));

                return true;
            }

            case ItemActionType.Glasses:
            {
                itemIsUnlocked = PlayerState.Instance()->IsGlassesUnlocked((ushort)GetGlassesID(item));

                return true;
            }
        }

        return true;
    }

    private void StoreItemUnlock(LuminaItem item)
    {
        if (item.ItemAction.RowId == 0)
        {
            return;
        }

        DalamudServices.PluginLog.Verbose($"Detected Item Completion with ID: {item.RowId}");

        switch ((ItemActionType)item.ItemAction.Value.Type)
        {
            case ItemActionType.Companion:
            case ItemActionType.BuddyEquip:
            case ItemActionType.Mount:
            case ItemActionType.SecretRecipeBook:
            case ItemActionType.UnlockLink:
            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<LuminaTrippleTriadCard>():
            case ItemActionType.FolkloreTome:
            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<LuminaOrchestrion>():
            case ItemActionType.FramersKit:
            case ItemActionType.Ornament:
            case ItemActionType.Glasses:
            {
                CollectedNewItem();

                break;
            }
        }
    }

    private List<LuminaItem> GetNewlyUnlockedItems(bool addToList = true)
    {
        List<LuminaItem> freshlyUnlockedItems = [];

        foreach (LuminaItem item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked))
            {
                continue;
            }

            if (!isUnlocked)
            {
                continue;
            }

            uint itemID = item.RowId;

            if (_unlockedItems.Contains(itemID))
            {
                continue;
            }

            freshlyUnlockedItems.Add(item);

            if (!addToList)
            {
                continue;
            }

            _unlockedItems.Add(item.RowId);
        }

        return freshlyUnlockedItems;
    }

    private void CollectedNewItem()
    {
        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnItemUnlock, ScreenshotReason.ItemUnlocked);
    }

    private void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float deltaTime)
    {
        if (DalamudServices.ObjectTable.LocalPlayer != null)
        {
            try
            {
                if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate) ||
                    module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate))
                {
                    DalamudServices.PluginLog.Verbose($"Unlocks Update Flag got set High: {module->AgentUpdateFlag}");

                    List<LuminaItem> unlockedItems = GetNewlyUnlockedItems();

                    foreach (LuminaItem item in unlockedItems)
                    {
                        DalamudServices.PluginLog.Verbose($"Detected Acquired Item with ID: {item.RowId} and the name: {item.Name.ExtractText()}");

                        StoreItemUnlock(item);
                    }
                }
            }
            catch (Exception ex)
            {
                DalamudServices.PluginLog.Error(ex, "Error during RaptureAtkModule_UpdateDetour");
            }
        }

        try
        {
            RaptureAtkModuleUpdateHook!.OriginalDisposeSafe(module, deltaTime);
        }
        catch (Exception e)
        {
            DalamudServices.PluginLog.Error(e, "Failed ATKModuleUpdate");
        }
    }

    public override void Reset()
    {
        _unlockedItems.Clear();

        foreach (LuminaItem item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked))
            {
                continue;
            }

            if (!isUnlocked)
            {
                continue;
            }

            _unlockedItems.Add(item.RowId);
        }
    }
}
