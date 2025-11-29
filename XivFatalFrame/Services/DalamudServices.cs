using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Reflection;

namespace XivFatalFrame.Services;

internal sealed class DalamudServices
{
                    internal string                  Version             { get; private set; } = null!;
                    internal XivFatalFramePlugin     XivFatalFramePlugin { get; private set; } = null!;
                    internal IDalamudPluginInterface DalamudPlugin       { get; private set; } = null!;
    [PluginService] internal IPluginLog              PluginLog           { get; private set; } = null!;
    [PluginService] internal IGameInteropProvider    Hooking             { get; private set; } = null!;
    [PluginService] internal IFramework              Framework           { get; private set; } = null!;
    [PluginService] internal IClientState            ClientState         { get; private set; } = null!;
    [PluginService] internal IObjectTable            ObjectTable         { get; private set; } = null!;
    [PluginService] internal IDutyState              DutyState           { get; private set; } = null!;
    [PluginService] internal IDataManager            DataManager         { get; private set; } = null!;
    [PluginService] internal ICommandManager         CommandManager      { get; private set; } = null!;
    [PluginService] internal IChatGui                ChatGui             { get; private set; } = null!;

    public static DalamudServices Create(IDalamudPluginInterface plugin, XivFatalFramePlugin petNicknames)
    {
        DalamudServices service = new DalamudServices();

        _ = plugin.Inject(service); // What am I going to do... log it? LMAOOOO                 you cant

        service.XivFatalFramePlugin = petNicknames;
        service.DalamudPlugin       = plugin;
        service.Version             = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        return service;
    }
}
