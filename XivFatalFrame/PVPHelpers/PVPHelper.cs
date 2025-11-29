using XivFatalFrame.PVPHelpers.Interfaces;

namespace XivFatalFrame.PVPHelpers;

internal class PVPHelper : IPVPSetter, IPVPReader
{
    public bool IsInPVP { get; private set; } = false;

    public void SetPVPState(bool isInPvp)
    {
        IsInPVP = isInPvp;
    }
}
