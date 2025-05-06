using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivFatalFrame.Hooking;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;
using XivFatalFrame.SteamAPI;
using XivFatalFrame.Windowing;

namespace XivFatalFrame;

public sealed class XivFatalFramePlugin : IDalamudPlugin
{
    private const    string                     FatalFrameCommand       = "/fatalframe";
    private          nint                       lastLocalUserAddress    = nint.Zero;
    private readonly DalamudServices            DalamudServices;
    private readonly Sheets                     Sheets;
    private readonly IDalamudPluginInterface    PluginInterface;
    private readonly Configuration              Configuration;
    private readonly ScreenshotTaker            ScreenshotTaker;
    private readonly FatalFrameEventHook        FatalFrameEventHook;
    private readonly WindowSystem               WindowSystem;
    private readonly FatalFrameConfigWindow     FatalFrameConfigWindow;
    private readonly SteamHelper                SteamHelper;

    public XivFatalFramePlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface         = pluginInterface;
        DalamudServices         = DalamudServices.Create(PluginInterface, this);

        SteamHelper             = new SteamHelper(DalamudServices.PluginLog);

        Sheets                  = new Sheets(DalamudServices);

        Configuration           = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration           .Initialize(PluginInterface);

        ScreenshotTaker         = new ScreenshotTaker(DalamudServices, Configuration, SteamHelper);
        ScreenshotTaker         .Init();

        FatalFrameEventHook     = new FatalFrameEventHook(DalamudServices, ScreenshotTaker, Configuration, Sheets);

        DalamudServices.CommandManager.AddHandler(FatalFrameCommand, new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = "Configuration Screen for FatalFrame",
            ShowInHelp = true,
        });

        WindowSystem            = new WindowSystem("FatalFrame");

        FatalFrameConfigWindow  = new FatalFrameConfigWindow(Configuration, DalamudServices, SteamHelper);

        WindowSystem.AddWindow(FatalFrameConfigWindow);

        PluginInterface.UiBuilder.Draw              += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi      += () => FatalFrameConfigWindow.IsOpen = true;

        DalamudServices.Framework.Update            += Update;
    }

    private void Update(IFramework framework)
    {
        ResetHandler();

        ScreenshotTaker.Update(framework);
        FatalFrameEventHook.Update(framework);
    }    

    private void ResetHandler()
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        if (lastLocalUserAddress == DalamudServices.ClientState.LocalPlayer.Address)
        {
            return;
        }

        lastLocalUserAddress = DalamudServices.ClientState.LocalPlayer.Address;

        FatalFrameEventHook.Reset();
    }

    private void OnCommand(string command, string args)
    {
        FatalFrameConfigWindow.Toggle();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        DalamudServices.CommandManager.RemoveHandler(FatalFrameCommand);
        DalamudServices.Framework.Update -= Update;
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        ScreenshotTaker.Dispose();
        FatalFrameEventHook.Dispose();

        SteamTimeline.Dispose();
    }
}
