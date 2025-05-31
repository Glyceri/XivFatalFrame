using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System.Numerics;
using XivFatalFrame.Services;

namespace XivFatalFrame.Windowing;

internal class FatalFrameConfigWindow : Window
{
    private readonly Configuration      Configuration;
    private readonly DalamudServices    DalamudServices;

    public FatalFrameConfigWindow(Configuration configuration, DalamudServices dalamudServices) : base("Fatal Frame", ImGuiWindowFlags.None, true)
    {
        Configuration   = configuration;
        DalamudServices = dalamudServices;

        Size            = new Vector2(280, 340);
        SizeCondition   = ImGuiCond.FirstUseEver;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(280, 340),
            MaximumSize = new Vector2(430, 900),
        };
    }

    public override void Draw()
    {
        ImGui.Text("Fatal Frame Version");

        string versionText = $"[{DalamudServices.Version}]";

        Vector2 textSize = ImGui.CalcTextSize(versionText);

        ImGui.SameLine();  

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - 30 - textSize.X - ImGui.GetStyle().ItemSpacing.X);

        ImGui.Text(versionText);

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.Coffee.ToIconString()))
        {
            Util.OpenLink("https://ko-fi.com/glyceri");
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Support me on Ko-Fi");
        }

        DrawPVPSetting("Take Screenshot On Death", ref Configuration.TakeScreenshotOnDeath);
        DrawSetting("Take Screenshot On Achievement", ref Configuration.TakeScreenshotOnAchievement);
        DrawSetting("Take Screenshot On Sightseeing Log", ref Configuration.TakeScreenshotOnEorzeaIncognita);
        DrawPVPSetting("Take Screenshot On Duty Completion", ref Configuration.TakeScreenshotOnDutyCompletion);
        DrawSetting("Take Screenshot On Level Up", ref Configuration.TakeScreenshotOnLevelup);
        DrawSetting("Take Screenshot On New Fish Caught", ref Configuration.TakeScreenshotOnFishCaught);
        DrawSetting("Take Screenshot On Quest Completion", ref Configuration.TakeScreenshotOnQuestComplete);
        DrawSetting("Take Screenshot On Item Unlocked", ref Configuration.TakeScreenshotOnItemUnlock);

        if (false) // Will turn into steam API stuff later
        {
            ImGui.NewLine();

            if (ImGui.Checkbox("Add event to Timeline on Steam##steamEvent", ref Configuration.AddMarkToSteamTimelines))
            {
                Configuration.Save();
            }
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

    bool DrawHeader(string header)
    {
        return ImGui.CollapsingHeader(header + $"##header{header}");
    }

    bool DrawCheckmark(string header, ref bool value)
    {
        return ImGui.Checkbox(header + $"##checkbox{header}", ref value);
    }

    bool DrawSlider(string header, ref float value)
    {
        return ImGui.SliderFloat($"After Delay##checkbox{header}", ref value, 0, 10);
    }

    void DrawBasicSetting(string header, ref SerializableSetting setting)
    {
        if (DrawCheckmark(header, ref setting.TakeScreenshot))
        {
            Configuration.Save();
        }

        ImGui.BeginDisabled(!setting.TakeScreenshot);

        if (DrawSlider(header, ref setting.AfterDelay))
        {
            Configuration.Save();
        }

        ImGui.EndDisabled();
    }

    void DrawSetting(string header, ref SerializableSetting setting)
    {
        if (!DrawHeader(header)) return;

        DrawBasicSetting(header, ref setting);
    }

    void DrawPVPSetting(string header, ref SerializablePvpSetting setting)
    {
        if (!DrawHeader(header)) return;

        SerializableSetting selfSetting = setting;

        DrawBasicSetting(header, ref selfSetting);

        string pvpHeader = header + " for PVP";

        if (DrawCheckmark(pvpHeader, ref setting.EnabledInPvp))
        {
            Configuration.Save();
        }

        ImGui.BeginDisabled(!setting.EnabledInPvp);

        if (DrawSlider(pvpHeader, ref setting.AfterDelayPVP))
        {
            Configuration.Save();
        }

        ImGui.EndDisabled();
    }
}
