using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace XivFatalFrame;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public SerializablePvpSetting TakeScreenshotOnDeath             = null!;
    public SerializableSetting    TakeScreenshotOnAchievement       = null!;
    public SerializableSetting    TakeScreenshotOnEorzeaIncognita   = null!;
    public SerializablePvpSetting TakeScreenshotOnDutyCompletion    = null!;
    public SerializableSetting    TakeScreenshotOnLevelup           = null!;
    public SerializableSetting    TakeScreenshotOnFishCaught        = null!;
    public SerializableSetting    TakeScreenshotOnQuestComplete     = null!;
    public SerializableSetting    TakeScreenshotOnItemUnlock        = null!;

    public bool AddMarkToSteamTimelines = true;

    public bool SilenceLog              = false;
    public bool CustomLogMessage        = true;

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;

        TakeScreenshotOnDeath           ??= new SerializablePvpSetting(takeScreenshot: true, enabledInPvp: true, afterDelay: 1.2f, afterDelayPVP: 0.1f);
        TakeScreenshotOnAchievement     ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 2.5f                     );
        TakeScreenshotOnEorzeaIncognita ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 0.6f                     );
        TakeScreenshotOnDutyCompletion  ??= new SerializablePvpSetting(takeScreenshot: true, enabledInPvp: true, afterDelay: 3.0f, afterDelayPVP: 0.1f);
        TakeScreenshotOnLevelup         ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 1.0f                     );
        TakeScreenshotOnFishCaught      ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 2.5f                     );
        TakeScreenshotOnQuestComplete   ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 1.2f                     );
        TakeScreenshotOnItemUnlock      ??= new SerializableSetting   (takeScreenshot: true,                     afterDelay: 0.6f                     );
    }

    public void Save()
    {
        PluginInterface?.SavePluginConfig(this);
    }
}
