using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System.Numerics;

namespace XivFatalFrame;

internal class FatalFrameConfigWindow : Window
{
    private readonly Configuration      Configuration;
    private readonly DalamudServices    DalamudServices;

    public FatalFrameConfigWindow(Configuration configuration, DalamudServices dalamudServices) : base("Fatal Frame", ImGuiWindowFlags.NoResize, true)
    {
        Configuration   = configuration;
        DalamudServices = dalamudServices;

        Size            = new Vector2(280, 340);

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(280, 340),
            MaximumSize = new Vector2(280, 340),
        };
    }

    public override void Draw()
    {
        ImGui.Text("Fatal Frame Version".PadRight(50) + $"[{DalamudServices.Version}]");

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - 30);

        if (ImGui.Button(FontAwesomeIcon.Coffee.ToIconString()))
        {
            Util.OpenLink("https://ko-fi.com/glyceri");
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Support me on Ko-Fi");
        }

        if (ImGui.Checkbox("Take Screenshot On Death##deathCheck",                          ref Configuration.TakeScreenshotOnDeath))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Achievement##achievementCheck",              ref Configuration.TakeScreenshotOnAchievement))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Sightseeing Log##eorzeaIncCheck",            ref Configuration.TakeScreenshotOnEorzeaIncognita))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Duty Completion##dutyCompletionCheck",       ref Configuration.TakeScreenshotOnDutyCompletion))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Level Up##levelupCheck",                     ref Configuration.TakeScreenshotOnLevelup))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On New Fish Caught##fishCheck",                 ref Configuration.TakeScreenshotOnFishCaught))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Quest Completion##questCheck",               ref Configuration.TakeScreenshotOnQuestComplete))
        {
            Configuration.Save();
        }

        if (ImGui.Checkbox("Take Screenshot On Item Unlocked##questCheck",                  ref Configuration.TakeScreenshotOnItemUnlock))
        {
            Configuration.Save();
        }

        ImGui.NewLine();

        if (ImGui.Checkbox("Silence Log##silenceLog",                                       ref Configuration.SilenceLog))
        {
            Configuration.Save();
        }

        ImGui.BeginDisabled(Configuration.SilenceLog);

        if (ImGui.Checkbox("Custom Log##customLog",                                         ref Configuration.CustomLogMessage))
        {
            Configuration.Save();
        }

        ImGui.EndDisabled();
    }
}
