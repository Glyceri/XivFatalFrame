using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivFatalFrame.Hooking;
using XivFatalFrame.PVPHelpers;
using XivFatalFrame.Screenshotter;
using XivFatalFrame.Services;
using XivFatalFrame.Windowing;

using DalamudCommandInfo = Dalamud.Game.Command.CommandInfo;

namespace XivFatalFrame;

public sealed class XivFatalFramePlugin : IDalamudPlugin
{
    private const    string                     FatalFrameCommand       = "/fatalframe";
    private          nint                       lastLocalUserAddress    = nint.Zero;
    private readonly DalamudServices            DalamudServices;
    private readonly Sheets                     Sheets;
    private readonly IDalamudPluginInterface    PluginInterface;
    private readonly Configuration              Configuration;
    private readonly PVPHelper                  PVPHelper;
    private readonly ScreenshotTaker            ScreenshotTaker;
    private readonly HookHandler                HookHandler;
    private readonly WindowSystem               WindowSystem;
    private readonly FatalFrameConfigWindow     FatalFrameConfigWindow;

    public XivFatalFramePlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface         = pluginInterface;
        DalamudServices         = DalamudServices.Create(PluginInterface, this);

        Sheets                  = new Sheets(DalamudServices);

        Configuration           = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration           .Initialize(PluginInterface);

        PVPHelper               = new PVPHelper();

        ScreenshotTaker         = new ScreenshotTaker(DalamudServices, Configuration, PVPHelper);
        ScreenshotTaker         .Init();

        HookHandler             = new HookHandler(DalamudServices, ScreenshotTaker, Configuration, Sheets, PVPHelper);

        _ = DalamudServices.CommandManager.AddHandler(FatalFrameCommand, new DalamudCommandInfo(OnCommand)
        {
            HelpMessage = "Configuration Screen for FatalFrame",
            ShowInHelp  = true,
        });

        WindowSystem            = new WindowSystem("FatalFrame");

        FatalFrameConfigWindow  = new FatalFrameConfigWindow(Configuration, DalamudServices);

        WindowSystem.AddWindow(FatalFrameConfigWindow);

        PluginInterface.UiBuilder.Draw              += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi      += () => FatalFrameConfigWindow.IsOpen = true;

        DalamudServices.Framework.Update            += Update;
    }

    private void Update(IFramework framework)
    {
        ResetHandler();

        ScreenshotTaker.Update(framework);
        HookHandler.Update(framework);
    }    

    private void ResetHandler()
    {
        if (DalamudServices.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        if (lastLocalUserAddress == DalamudServices.ObjectTable.LocalPlayer.Address)
        {
            return;
        }

        lastLocalUserAddress = DalamudServices.ObjectTable.LocalPlayer.Address;

        HookHandler.Reset();
    }

    private void OnCommand(string command, string args)
    {
        FatalFrameConfigWindow.Toggle();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        _ = DalamudServices.CommandManager.RemoveHandler(FatalFrameCommand);

        DalamudServices.Framework.Update -= Update;
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        ScreenshotTaker.Dispose();
        HookHandler.Dispose();
    }
}
