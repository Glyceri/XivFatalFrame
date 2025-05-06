using System;
using Newtonsoft.Json;

namespace XivFatalFrame;

[Serializable]
public class SerializableSetting
{
    public bool  TakeScreenshot;
    public float AfterDelay;

    [JsonConstructor]
    public SerializableSetting(bool takeScreenshot, float afterDelay)
    {
        TakeScreenshot = takeScreenshot;
        AfterDelay     = afterDelay;
    }
}
