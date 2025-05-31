using System.Runtime.InteropServices;

namespace XivFatalFrame.SteamAPI;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SteamTimelineVTable
{
    public delegate* unmanaged<SteamTimeline*, char*, float, void>                                                          SetTimelineTooltip;
    public delegate* unmanaged<SteamTimeline*, float, void>                                                                 ClearTimelineTooltip;
    public delegate* unmanaged<SteamTimeline*, ETimelineGameMode, void>                                                     SetTimelineGameMode;
    public delegate* unmanaged<SteamTimeline*, char*, char*, char*, uint, float, ETimelineEventClipPriority, nint>          AddInstantaneousTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, char*, char*, char*, uint, float, float, ETimelineEventClipPriority, nint>   AddRangeTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, char*, char*, char*, uint, float, ETimelineEventClipPriority, nint>          StartRangeTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, nint, char*, char*, char*, uint, ETimelineEventClipPriority, void>           UpdateRangeTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, nint, float, void>                                                           EndRangeTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, nint, void>                                                                  RemoveTimelineEvent;
    public delegate* unmanaged<SteamTimeline*, nint, nint>                                                                  DoesEventRecordingExist;
    public delegate* unmanaged<SteamTimeline*, void>                                                                        StartGamePhase;
    public delegate* unmanaged<SteamTimeline*, void>                                                                        EndGamePhase;
    public delegate* unmanaged<SteamTimeline*, char*, void>                                                                 SetGamePhaseID;
    public delegate* unmanaged<SteamTimeline*, char*, nint>                                                                 DoesGamePhaseRecordingExist;
    public delegate* unmanaged<SteamTimeline*, char*, char*, char*, uint, void>                                             AddGamePhaseTag;
    public delegate* unmanaged<SteamTimeline*, char*, char*, uint, void>                                                    SetGamePhaseAttribute;
    public delegate* unmanaged<SteamTimeline*, char*, void>                                                                 OpenOverlayToGamePhase;
    public delegate* unmanaged<SteamTimeline*, nint, void>                                                                  OpenOverlayToTimelineEvent;
}
