using System.Runtime.InteropServices;
using System.Text;

namespace XivFatalFrame.SteamAPI;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe partial struct SteamTimeline
{
    [FieldOffset(0x0)] public SteamTimelineVTable* VTable;

    public void SetTimelineTooltip(string description, float timeDelta = 0)
    {
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            VTable->SetTimelineTooltip(self, (char*)descriptionPtr, timeDelta);
        }
    }

    public void ClearTimelineTooltip(float timeDelta = 0)
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->ClearTimelineTooltip(self, timeDelta);
        }
    }

    public void SetTimelineGameMode(ETimelineGameMode mode)
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->SetTimelineGameMode(self, mode);
        }
    }

    public nint AddInstantaneousTimelineEvent(string title, string description, string icon, uint iconPriority = 0, float startOffsetSeconds = 0, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None)
    {
        fixed (byte* titlePtr       = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr        = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            return VTable->AddInstantaneousTimelineEvent(self, (char*)titlePtr, (char*)descriptionPtr, (char*)iconPtr, iconPriority, startOffsetSeconds, possibleClip);
        }
    }

    public nint AddRangeTimelineEvent(string title, string description, string icon, uint iconPriority = 0, float startOffsetSeconds = 0, float duration = 0, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None)
    {
        fixed (byte* titlePtr       = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr        = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self  = &this)
        { 
            return VTable->AddRangeTimelineEvent(self, (char*)titlePtr, (char*)descriptionPtr, (char*)iconPtr, iconPriority, startOffsetSeconds, duration, possibleClip);
        }
    }

    public nint StartRangeTimelineEvent(string title, string description, string icon, uint priority = 0, float startOffsetSeconds = 0, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None)
    {
        fixed (byte* titlePtr       = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr        = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            return VTable->StartRangeTimelineEvent(self, (char*)titlePtr, (char*)descriptionPtr, (char*)iconPtr, priority, startOffsetSeconds, possibleClip);
        }
    }

    public void UpdateRangeTimelineEvent(nint @event, string title, string description, string icon, uint priority = 0, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None)
    {
        fixed (byte* titlePtr       = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr        = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            VTable->UpdateRangeTimelineEvent(self, @event, (char*)titlePtr, (char*)descriptionPtr, (char*)iconPtr, priority, possibleClip);
        }
    }

    public void EndRangeTimelineEvent(nint @event, float endOffsetSeconds = 0)
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->EndRangeTimelineEvent(self, @event, endOffsetSeconds);
        }
    }

    public void RemoveTimelineEvent(nint @event)
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->RemoveTimelineEvent(self, @event);
        }
    }

    public void StartGamePhase()
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->StartGamePhase(self);
        }
    }

    public void EndGamePhase()
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->EndGamePhase(self);
        }
    }

    public void SetGamePhaseId(string phaseId)
    {
        fixed (byte* phaseIdPtr     = Encoding.UTF8.GetBytes(phaseId + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            VTable->SetGamePhaseID(self, (char*)phaseIdPtr);
        }
    }

    public void AddGamePhaseTag(string tagName, string tagIcon, string tagGroup, uint priority = 0)
    {
        fixed (byte* tagNamePtr     = Encoding.UTF8.GetBytes(tagName + "\0"))
        fixed (byte* tagIconPtr     = Encoding.UTF8.GetBytes(tagIcon + "\0"))
        fixed (byte* tagGroupPtr    = Encoding.UTF8.GetBytes(tagGroup + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            VTable->AddGamePhaseTag(self, (char*)tagNamePtr, (char*)tagIconPtr, (char*)tagGroupPtr, priority);
        }
    }

    public void SetGamePhaseAttribute(string attributeGroup, string attributeValue, uint priority = 0)
    {
        fixed (byte* attributeGroupPtr  = Encoding.UTF8.GetBytes(attributeGroup + "\0"))
        fixed (byte* attributeValuePtr  = Encoding.UTF8.GetBytes(attributeValue + "\0"))
        fixed (SteamTimeline* self      = &this)
        {
            VTable->SetGamePhaseAttribute(self, (char*)attributeGroupPtr, (char*)attributeValuePtr, priority);
        }
    }

    public void OpenOverlayToGamePhase(string phaseId)
    {
        fixed (byte* phaseIdPtr     = Encoding.UTF8.GetBytes(phaseId + "\0"))
        fixed (SteamTimeline* self  = &this)
        {
            VTable->OpenOverlayToGamePhase(self, (char*)phaseIdPtr);
        }
    }

    public void OpenOverlayToTimelineEvent(nint @event)
    {
        fixed (SteamTimeline* self = &this)
        {
            VTable->OpenOverlayToTimelineEvent(self, @event);
        }
    }

    private static SteamTimeline* Instance;

    private delegate uint RestartAppIfNecessaryDelegate(uint appId);
    private delegate nint InitDelegate();
    private delegate uint GetHSteamUserDelegate();
    private delegate nint FindOrCreateUserInterfaceDelegate(uint steamUser, char* version);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint LoadLibraryW(string fileName);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetProcAddress(nint module, string procName);

    public static SteamTimeline* Get(IPluginLog pluginLog, uint? appId = null)
    {
        if (Instance != null)
        {
            return Instance;
        }

        if (appId == null)
        {
            pluginLog.Debug("appId is null");
            return null;
        }

        Framework* framework = Framework.Instance();
        if (framework == null)
        {
            pluginLog.Debug("Framework was null");
            return null;
        }

        bool steamApiInitialized = framework->IsSteamApiInitialized();
        if (!steamApiInitialized)
        {
            pluginLog.Debug("Steam API was not initialized.");
            return null;
        }

        nint handle = framework->SteamApiLibraryHandle;
        if (handle == nint.Zero)
        {
            pluginLog.Debug("Steam API library handle was null");
            return null;
        }

        nint getHSteamUserAddr = GetProcAddress(handle, "SteamAPI_GetHSteamUser");
        if (getHSteamUserAddr == nint.Zero)
        {
            pluginLog.Debug("GetHSteamUser addr was null");
            return null;
        }

        GetHSteamUserDelegate getHSteamUser = Marshal.GetDelegateForFunctionPointer<GetHSteamUserDelegate>(getHSteamUserAddr);

        uint hSteamUser = getHSteamUser();

        pluginLog.Debug("Got HSteamUser: {0}", hSteamUser);

        nint findOrCreateUserInterfaceAddr = GetProcAddress(handle, "SteamInternal_FindOrCreateUserInterface");
        if (findOrCreateUserInterfaceAddr == nint.Zero)
        {
            pluginLog.Debug("FindOrCreateUserInterface addr was null");
            return null;
        }

        FindOrCreateUserInterfaceDelegate findOrCreateUserInterface = Marshal.GetDelegateForFunctionPointer<FindOrCreateUserInterfaceDelegate>(findOrCreateUserInterfaceAddr);

        fixed (byte* name = "STEAMTIMELINE_INTERFACE_V004\0"u8)
        {
            nint @interface = findOrCreateUserInterface(hSteamUser, (char*)name);
            if (@interface == nint.Zero)
            {
                pluginLog.Debug("Interface was null");
                return null;
            }

            SteamTimeline* @struct = (SteamTimeline*)@interface;
            if (@struct->VTable == null)
            {
                pluginLog.Debug("Interface VTable was null");
                return null;
            }

            Instance = (SteamTimeline*)@interface;
            pluginLog.Debug("Got interface: {0}", @interface.ToString("X8"));

            return Instance;
        }
    }

    public static void Dispose()
    {
        Instance = null;
    }
}
