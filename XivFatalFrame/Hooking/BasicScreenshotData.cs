namespace XivFatalFrame.Hooking;

public readonly struct BasicScreenshotData
{
    public readonly uint    ClassId;
    public readonly byte    Level;
    public readonly uint    CurrentMap;
    public readonly ushort  CurrentTerritoryType;
    public readonly byte    CurrentWeather;
    public readonly long    EorzeanTime;

    public BasicScreenshotData() { }

    public BasicScreenshotData(uint classId, byte level, uint currentMap, ushort currentTerritoryType, byte currentWeather, long eorzeanTime)
    {
        ClassId                 = classId;
        Level                   = level;
        CurrentMap              = currentMap;
        CurrentTerritoryType    = currentTerritoryType;
        CurrentWeather          = currentWeather;
        EorzeanTime             = eorzeanTime;
    }
}
