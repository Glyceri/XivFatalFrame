using System;
using Newtonsoft.Json;

namespace XivFatalFrame;

[Serializable]
public class SerializablePvpSetting : SerializableSetting
{
    public bool  EnabledInPvp;
    public float AfterDelayPVP;

    [JsonConstructor]
    public SerializablePvpSetting(bool takeScreenshot, bool enabledInPvp, float afterDelay, float afterDelayPVP) : base(takeScreenshot, afterDelay)
    {
        EnabledInPvp  = enabledInPvp;
        AfterDelayPVP = afterDelayPVP;
    }
}
