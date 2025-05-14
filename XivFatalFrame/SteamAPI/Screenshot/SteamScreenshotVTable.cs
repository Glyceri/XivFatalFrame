using System.Runtime.InteropServices;

namespace XivFatalFrame.SteamAPI.Screenshot;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SteamScreenshotVTable
{
    public delegate* unmanaged<SteamScreenshot*, void*, uint, int, int, void>           WriteScreenshot;
    public delegate* unmanaged<SteamScreenshot*, char*, char*, int, int, uint>          AddScreenshotToLibrary;
    public delegate* unmanaged<SteamScreenshot*, void>                                  TriggerScreenshot;
    public delegate* unmanaged<SteamScreenshot*, bool, void>                            HookScreenshots;
    public delegate* unmanaged<SteamScreenshot*, uint, char*, bool>                     SetLocation;
    public delegate* unmanaged<SteamScreenshot*, uint, ulong, bool>                     TagUser;
    public delegate* unmanaged<SteamScreenshot*, uint, ulong, bool>                     TagPublishedFile;
    public delegate* unmanaged<SteamScreenshot*, bool>                                  IsScreenshotHooked;
    public delegate* unmanaged<SteamScreenshot*, EVRScreenshotType, char*, char*, uint> AddVRScreenshotToLibrary;
}
