using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using XivFatalFrame.Hooking.Death;
using XivFatalFrame.PVPHelpers.Interfaces;
using XivFatalFrame.ScreenshotDatabasing;
using XivFatalFrame.ScreenshotDatabasing.ScreenshotParameters;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.ActionEffectHandler;

namespace XivFatalFrame.Hooking;

internal unsafe class FatalFrameEventHook : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly ScreenshotTaker    ScreenshotTaker;
    private readonly Configuration      Configuration;
    private readonly Sheets             Sheets;
    private readonly IPVPSetter         PVPSetter;
    private readonly ScreenshotDatabase ScreenshotDatabase;

    private const uint SpearFishIdOffset = 20000;

    private          byte[]         _fishStore                  = [];
    private          byte[]         _spearFishStore             = [];
    private          short[]        _currentClassJobLevels      = [];
    private readonly List<uint>     _questsCompleted            = [];
    private readonly List<uint>     _unlockedItems              = [];
    private          byte           _lastAcceptedQuestCount     = 0;

    private          DamageEvent?   lastDamageEvent             = null;
    private       IPlayerCharacter? lastPlayerCharacter         = null;

    private delegate void OnAchievementUnlockDelegate       (Achievement* achievement, uint achievementID);
    private delegate nint VistaUnlockedDelegate             (ushort index, int a2, int a3);
    private delegate void RaptureAtkModuleUpdateDelegate    (RaptureAtkModule* ram, float deltaTime);
    private delegate void ProcessPacketActionEffectDelegate (uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targetEntityIds);
    private delegate void ProcessPacketActorControlDelegate (uint entityId, ActorControlCategory type, uint param1, uint param2, uint param3, uint param4, uint param5, uint param6, ulong param7, bool isReplay);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53",        DetourName = nameof(AchievementUnlockedDetour))]
    private readonly Hook<OnAchievementUnlockDelegate>?         AchievementUnlockingHook = null;

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8B 4C 24 70 E8",  DetourName = nameof(OnVistaUnlockedDetour))]
    private readonly Hook<VistaUnlockedDelegate>?               VistaUnlockHook = null;

    private readonly Hook<RaptureAtkModuleUpdateDelegate>?      RaptureAtkModuleUpdateHook;

    private readonly Hook<ProcessPacketActionEffectDelegate>?   ProcessPacketActionEffectHook;

    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64",              DetourName = nameof(ProcessPacketActorControlDetour))]
    private readonly Hook<ProcessPacketActorControlDelegate>?   ProcessPacketActorControlHook = null!;

    public FatalFrameEventHook(DalamudServices dalamudServices, ScreenshotTaker screenshotTaker, Configuration configuration, Sheets sheets, IPVPSetter pvpSetter, ScreenshotDatabase screenshotDatabase)
    {
        DalamudServices     = dalamudServices;
        ScreenshotTaker     = screenshotTaker;
        Configuration       = configuration;
        Sheets              = sheets;
        PVPSetter           = pvpSetter;
        ScreenshotDatabase  = screenshotDatabase;

        DalamudServices.Hooking.InitializeFromAttributes(this);

        RaptureAtkModuleUpdateHook      = DalamudServices.Hooking.HookFromAddress<RaptureAtkModuleUpdateDelegate>((nint)RaptureAtkModule.StaticVirtualTablePointer->Update, RaptureAtkModule_UpdateDetour);

        ProcessPacketActionEffectHook   = DalamudServices.Hooking.HookFromSignature<ProcessPacketActionEffectDelegate>(ActionEffectHandler.Addresses.Receive.String, ProcessPacketActionEffectDetour);

        Reset();

        AchievementUnlockingHook?   .Enable();
        VistaUnlockHook?            .Enable();
        RaptureAtkModuleUpdateHook? .Enable();

        ProcessPacketActionEffectHook?.Enable();
        ProcessPacketActorControlHook?.Enable();

        DalamudServices.DutyState.DutyCompleted         += OnDutyCompleted;
        DalamudServices.ClientState.LevelChanged        += OnLevelChanged;
        DalamudServices.ClientState.Login               += OnLogin;
    }

    private BasicScreenshotData CreateBasicData()
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return new BasicScreenshotData();
        }

        return new BasicScreenshotData
        (
            DalamudServices.ClientState.LocalPlayer.ClassJob.RowId,
            DalamudServices.ClientState.LocalPlayer.Level,
            DalamudServices.ClientState.MapId,
            DalamudServices.ClientState.TerritoryType,
            WeatherManager.Instance()->GetCurrentWeather(),
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->ClientTime.EorzeaTime
        );
    }

    private void OnLogin()
    {
        DalamudServices.PluginLog.Verbose($"Detected a login, Reset() has been called.");

        Reset();
    }

    public void Update(IFramework framework)
    {
        HandleClientPVPState();
        CheckFishy();
        CheckQuests();

        if (lastPlayerCharacter != DalamudServices.ClientState.LocalPlayer)
        {
            lastPlayerCharacter = DalamudServices.ClientState.LocalPlayer;

            ScreenshotDatabase.SetActiveUser(lastPlayerCharacter);
        }
    }

    public void Reset()
    {
         _fishStore                 = PlayerState.Instance()->CaughtFishBitmask.ToArray();
        _spearFishStore             = PlayerState.Instance()->CaughtSpearfishBitmask.ToArray();
        _currentClassJobLevels      = PlayerState.Instance()->ClassJobLevels.ToArray();
        _lastAcceptedQuestCount     = QuestManager.Instance()->NumAcceptedQuests;

        _questsCompleted.Clear();

        foreach (Lumina.Excel.Sheets.Quest quest in Sheets.AllQuests)
        {
            if (!QuestManager.IsQuestComplete(quest.RowId)) continue;

            _questsCompleted.Add(quest.RowId);
        }

        _unlockedItems.Clear();

        foreach (Lumina.Excel.Sheets.Item item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked)) continue;
            if (!isUnlocked) continue;

            _unlockedItems.Add(item.RowId);
        }
    }

    private void ProcessPacketActionEffectDetour(uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* effectHeader, ActionEffectHandler.TargetEffects* effectArray, GameObjectId* targetEntityIds)
    {
        ProcessPacketActionEffectHook!.Original(casterEntityId, casterPtr, targetPos, effectHeader, effectArray, targetEntityIds);

        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        ulong localPlayerId = DalamudServices.ClientState.LocalPlayer.GameObjectId;

        try
        {
            if (effectHeader->NumTargets == 0)
            {
                return;
            }

            for (int i = 0; i < effectHeader->NumTargets; i++)
            {
                GameObjectId* gObjectId = &targetEntityIds[i];
                if (gObjectId == null)
                {
                    continue;
                }

                if (localPlayerId != gObjectId->Id)
                {
                    continue;
                }

                for (int f = 0; f < 8; f++)
                {
                    ref Effect actionEffect = ref effectArray[i].Effects[f];

                    if (actionEffect.Type == 0)
                    {
                        continue;
                    }

                    uint amount = actionEffect.Value;

                    if ((actionEffect.Param4 & 0x40) == 0x40)
                    {
                        amount += (uint)actionEffect.Param3 << 16;
                    }

                    uint actionId = effectHeader->ActionType switch
                    {
                        ActionType.Mount => 0xD000000 + effectHeader->ActionId,
                        ActionType.Item  => 0x2000000 + effectHeader->ActionId,
                        _                => effectHeader->SpellId
                    };

                    if ((ActionEffectType)actionEffect.Type != ActionEffectType.Damage)
                    {
                        continue;
                    }

                    Lumina.Excel.Sheets.Action? action = Sheets.GetAction(actionId);

                    if (action == null)
                    {
                        continue;
                    }

                    lastDamageEvent = new DamageEvent(casterPtr->NameString, action.Value.RowId, amount);
                }
            }
        }
        catch (Exception ex)
        {
            DalamudServices.PluginLog.Error(ex, "Error in ProcessPacketActionEffectDetour");
        }
    }

    private void ProcessPacketActorControlDetour(uint entityId, ActorControlCategory type, uint param1, uint amount, uint param3, uint param4, uint param5, uint param6, ulong param7, bool flag)
    {
        ProcessPacketActorControlHook!.Original(entityId, type, param1, amount, param3, param4, param5, param6, param7, flag);

        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        uint localPlayerId = DalamudServices.ClientState.LocalPlayer.EntityId;

        if (entityId != localPlayerId)
        {
            return;
        }

        if (type == ActorControlCategory.DoT)
        {
            lastDamageEvent = new DamageEvent(amount);
        }

        if (type == ActorControlCategory.Death)
        {
            DalamudServices.PluginLog.Verbose($"You died and I think from this: {lastDamageEvent}");

            ScreenshotTaker.TakeScreenshot
            (
                Configuration.TakeScreenshotOnDeath,
                new DeathParameters
                (
                    CreateBasicData(),
                    lastDamageEvent?.SourceName ?? string.Empty,
                    lastDamageEvent?.Action ?? 0,
                    lastDamageEvent?.Amount ?? 0
                )
            );
        }
    }
    
    private void HandleClientPVPState()
    {
        PVPSetter.SetPVPState(false);

        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        if (!DalamudServices.ClientState.LocalPlayer.IsValid())
        {
            return;
        }

        PVPSetter.SetPVPState(DalamudServices.ClientState.IsPvP);
    }

    private void CheckFishy()
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        uint? fishOutcome = CheckFishies(ref _fishStore, PlayerState.Instance()->CaughtFishBitmask);
        if (fishOutcome != null)
        {
            DalamudServices.PluginLog.Information($"Found new fish caught with ID: {fishOutcome.Value}");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnFishCaught, new FishParameters(CreateBasicData(), false, fishOutcome.Value));
        }

        uint? spfishOutcome = CheckFishies(ref _spearFishStore, PlayerState.Instance()->CaughtSpearfishBitmask);
        if (spfishOutcome != null)
        {
            spfishOutcome += SpearFishIdOffset;
            DalamudServices.PluginLog.Information($"Found new spearfish caught with ID: {spfishOutcome.Value}");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnFishCaught, new FishParameters(CreateBasicData(), true, spfishOutcome.Value));
        }
    }

    private void AchievementUnlockedDetour(Achievement* achievement, uint achievementID)
    {
        DalamudServices.PluginLog.Information($"Detected Acquired Achievement with ID: {achievementID}");
        
        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnAchievement, new AchievementParameters(CreateBasicData(), achievementID));

        AchievementUnlockingHook!.Original(achievement, achievementID);
    }

    private nint OnVistaUnlockedDetour(ushort index, int a2, int a3)
    {
        DalamudServices.PluginLog.Information($"Detected a vista unlocked at index: {index}");

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnEorzeaIncognita, new SightseeingParameters(CreateBasicData(), index));

        return VistaUnlockHook!.Original(index, a2, a3);
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
        if (fishyEquals) return null;

        uint? outcome = GetCaughtFishIndices(store, span);

        store = span.ToArray();

        return outcome;
    }

    private void CheckQuests()
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        byte numAcceptedQuests = QuestManager.Instance()->NumAcceptedQuests;

        if (numAcceptedQuests == _lastAcceptedQuestCount)
        {
            return;
        }

        _lastAcceptedQuestCount = numAcceptedQuests;

        foreach (Lumina.Excel.Sheets.Quest quest in Sheets.AllQuests)
        {
            uint questRowID = quest.RowId;
            if (!QuestManager.IsQuestComplete(questRowID)) continue;
            if (_questsCompleted.Contains(questRowID)) continue;

            _questsCompleted.Add(questRowID);
            DalamudServices.PluginLog.Information($"Quest with ID {questRowID} and name {quest.Name.ExtractText()} has been found.");

            ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnQuestComplete, new QuestParameters(CreateBasicData(), questRowID));
        }
    }

    private void OnDutyCompleted(object? _, ushort dutyID)
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnDutyCompletion, new DutyParameters(CreateBasicData(), dutyID));
    }

    private void OnLevelChanged(uint classJobId, uint level)
    {
        DalamudServices.PluginLog.Verbose($"Detected a level change on the job: {classJobId} to level: {level}");

        Lumina.Excel.Sheets.ClassJob? classJob = Sheets.GetClassJob(classJobId);
        if (classJob == null)
        {
            DalamudServices.PluginLog.Information("Couldn't find the classjob in the sheets???? HOW");
            return;
        }

        sbyte arrayIndex = classJob.Value.ExpArrayIndex;
        if (arrayIndex < 0 || arrayIndex >= _currentClassJobLevels.Length)
        {
            DalamudServices.PluginLog.Information($"Array index is out of range: {arrayIndex} on the classJobArray: {_currentClassJobLevels.Length}");
            return;
        }

        short currentLevel = _currentClassJobLevels[arrayIndex];
        if (currentLevel >= level)
        {
            DalamudServices.PluginLog.Information($"This resulted in no actual change.");
            return;
        }

        Reset();

        DalamudServices.PluginLog.Information($"The class: {classJobId}, {arrayIndex}, {classJob.Value.Name.ExtractText()} leveled up from: {currentLevel} to {level}. This has been marked.");

        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnLevelup, new LevelUpParameters(CreateBasicData(), (byte)classJobId, (byte)level));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetCompanionID          (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetBuddyEquipID         (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetMountID              (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetSecretRecipeID       (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetUnlockLinkID         (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetFolkloreID           (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetOrnamentID           (Lumina.Excel.Sheets.Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private uint     GetGlassesID            (Lumina.Excel.Sheets.Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private uint     GetTrippleTriadID       (Lumina.Excel.Sheets.Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private uint     GetOrchestrionID        (Lumina.Excel.Sheets.Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private uint     GetFramerKitID          (Lumina.Excel.Sheets.Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private ushort   GetItemActionID         (Lumina.Excel.Sheets.Item item) => item.ItemAction.Value.Data[0];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private uint     GetItemAdditionalDataID (Lumina.Excel.Sheets.Item item) => item.AdditionalData.RowId;

    private bool IsUnlocked(Lumina.Excel.Sheets.Item item, out bool itemIsUnlocked)
    {
        itemIsUnlocked = false;

        if (item.ItemAction.RowId == 0) return false;

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
                itemIsUnlocked = PlayerState.Instance()->IsSecretRecipeBookUnlocked(GetSecretRecipeID(item));
                return true;

            case ItemActionType.UnlockLink:
            {
                itemIsUnlocked = UIState.Instance()->IsUnlockLinkUnlocked(GetUnlockLinkID(item));
                return true;
            }

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<Lumina.Excel.Sheets.TripleTriadCard>():
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

    private void StoreItemUnlock(Lumina.Excel.Sheets.Item item)
    {
        if (item.ItemAction.RowId == 0) return;

        DalamudServices.PluginLog.Verbose($"Detected Item Completion with ID: {item.RowId}");

        switch ((ItemActionType)item.ItemAction.Value.Type)
        {
            case ItemActionType.Companion:
            case ItemActionType.BuddyEquip:
            case ItemActionType.Mount:
            case ItemActionType.SecretRecipeBook:
            case ItemActionType.UnlockLink:
            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<Lumina.Excel.Sheets.TripleTriadCard>():
            case ItemActionType.FolkloreTome:
            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Lumina.Excel.Sheets.Orchestrion>():
            case ItemActionType.FramersKit:
            case ItemActionType.Ornament:
            case ItemActionType.Glasses:
            {
                CollectedNewItem(item);
                break;
            }
        }
    }

    private List<Lumina.Excel.Sheets.Item> GetNewlyUnlockedItems(bool addToList = true)
    {
        List<Lumina.Excel.Sheets.Item> freshlyUnlockedItems = new List<Lumina.Excel.Sheets.Item>();

        foreach (Lumina.Excel.Sheets.Item item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked)) continue;
            if (!isUnlocked) continue;

            uint itemID = item.RowId;
            if (_unlockedItems.Contains(itemID)) continue;

            freshlyUnlockedItems.Add(item);

            if (!addToList) continue;
            _unlockedItems.Add(item.RowId);
        }

        return freshlyUnlockedItems;
    }

    private void CollectedNewItem(Lumina.Excel.Sheets.Item item)
    {
        ScreenshotTaker.TakeScreenshot(Configuration.TakeScreenshotOnItemUnlock, new ItemParameters(CreateBasicData(), item.RowId));
    }

    private void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float deltaTime)
    {
        if (DalamudServices.ClientState.LocalPlayer != null)
        {
            try
            {
                if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate) ||
                    module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate))
                {
                    DalamudServices.PluginLog.Verbose($"Unlocks Update Flag got set High: {module->AgentUpdateFlag}");
                    List<Lumina.Excel.Sheets.Item> unlockedItems = GetNewlyUnlockedItems();

                    foreach (Lumina.Excel.Sheets.Item item in unlockedItems)
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

    public void Dispose()
    {
        DalamudServices.DutyState.DutyCompleted     -= OnDutyCompleted;
        DalamudServices.ClientState.LevelChanged    -= OnLevelChanged;
        DalamudServices.ClientState.Login           -= OnLogin;

        AchievementUnlockingHook?   .Dispose();
        VistaUnlockHook?            .Dispose();
        RaptureAtkModuleUpdateHook? .Dispose();

        ProcessPacketActionEffectHook?.Dispose();
        ProcessPacketActorControlHook?.Dispose();
    }
}
