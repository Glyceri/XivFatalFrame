using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace XivFatalFrame;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool TakeScreenshotOnDeath                   = true;
    public bool TakeScreenshotOnAchievement             = true;
    public bool TakeScreenshotOnEorzeaIncognita         = true;
    public bool TakeScreenshotOnDutyCompletion          = true;
    public bool TakeScreenshotOnLevelup                 = true;
    public bool TakeScreenshotOnQuestComplete           = true; 
    public bool TakeScreenshotOnItemUnlock              = true;

    public bool SilenceLog                              = false;
    public bool CustomLogMessage                        = true;

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface?.SavePluginConfig(this);
    }
}
