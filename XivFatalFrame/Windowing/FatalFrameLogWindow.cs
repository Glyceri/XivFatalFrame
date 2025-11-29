using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using XivFatalFrame.ScreenshotDatabasing;
using XivFatalFrame.Services;

namespace XivFatalFrame.Windowing;

internal class FatalFrameLogWindow : Window, IDisposable
{
    private readonly Configuration      Configuration;
    private readonly DalamudServices    DalamudServices;
    private readonly ScreenshotDatabase ScreenshotDatabase;

    private int imageOffset     = 0;

    private float[] animationTimes = new float[9];

    private IDalamudTextureWrap SearchTexture;
    private IDalamudTextureWrap DateBar;

    private int selectedIndex = -1;

    bool fullScreenMode = false;

    public FatalFrameLogWindow(Configuration configuration, DalamudServices dalamudServices, ScreenshotDatabase screenshotDatabase) : base("Fatal Frame Log", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse, true)
    {
        IsOpen = true;

        Configuration       = configuration;
        DalamudServices     = dalamudServices;
        ScreenshotDatabase  = screenshotDatabase;

        Size            = new Vector2(1920, 1080);
        SizeCondition   = ImGuiCond.FirstUseEver;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(1920, 1080) * 0.5f,
            MaximumSize = new Vector2(5120, 2880),
        };

        SearchTexture   = DalamudServices.TextureProvider.GetFromGameIcon(194019).RentAsync().Result;
        DateBar         = DalamudServices.TextureProvider.GetFromGameIcon(196255).RentAsync().Result;
    }

    string text = string.Empty;

    private DatabaseEntry[] AllEntries()
    {
        List<DatabaseEntry> entries = new List<DatabaseEntry>();

        foreach (DatabaseUser user in ScreenshotDatabase.GetAllUsers())
        {
            foreach(DatabaseEntry entry in user.Entries)
            {
                entries.Add(entry);
            }
        }

        return entries.ToArray();
    }

    private DateTime lastTime = DateTime.UtcNow;

    public override void Draw()
    {
        DateTime now    = DateTime.UtcNow;
        TimeSpan delta  = now - lastTime;

        float deltaTime = (float)delta.TotalSeconds; // Delta in seconds

        lastTime = now;

        DatabaseEntry[] entries = AllEntries();

        if (fullScreenMode)
        {
            Vector2 oldPos = ImGui.GetCursorPos();

            ISharedImmediateTexture currentTexture = DalamudServices.TextureProvider.GetFromFile(entries[selectedIndex].ScreenshotPath);

            IDalamudTextureWrap usedTexture = SearchTexture;

            if (currentTexture.TryGetWrap(out IDalamudTextureWrap? texture, out Exception? exception))
            {
                usedTexture = texture;
            }

            if (exception != null)
            {
                DalamudServices.PluginLog.Error(exception, "Exception when trying to get texture from file");
            }

            Vector2 size = ImGui.GetContentRegionAvail() - ImGui.GetStyle().ItemSpacing - ImGui.GetStyle().FramePadding;

            if (ImGui.ImageButton(usedTexture.ImGuiHandle, size))
            {
                fullScreenMode = false;
            }

            Vector2 newLinePos = ImGui.GetCursorPos();

            ImGui.SameLine();

            Vector2 curPos = ImGui.GetCursorPos();

            Vector2 diff = newLinePos - curPos;

            float height = 400;

            float res = DateBar.Height / height;

            ImGui.SetCursorPos(new Vector2(oldPos.X, curPos.Y + diff.Y - DateBar.Height * res * 2));

            ImGui.Image(DateBar.ImGuiHandle, new Vector2(size.X, height));

            ImGui.SetCursorPos(curPos);

            return;
        }

        int entryCount = entries.Length;

        int pageCount = (int)MathF.Ceiling(entryCount / 9.0f);

        pageCount -= 1;

        if (pageCount < 0)
        {
            pageCount = 0;
        }

        if (ImGui.BeginListBox("##ImageLogDisplay", new Vector2(ImGui.GetContentRegionAvail().X * 0.7f, ImGui.GetContentRegionAvail().Y)))
        {
            ImGui.BeginDisabled(imageOffset == 0);

            if (ImGui.Button("Previous"))
            {
                UpdateImageOffset(-1, pageCount);
            }

            ImGui.EndDisabled();

            ImGui.SameLine();

            ImGui.BeginDisabled(imageOffset >= pageCount);

            if (ImGui.Button("Next"))
            {
                UpdateImageOffset(1, pageCount);
            }

            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.InputText("Search##fatalframesearch", ref text, 64))
            {

            }

            if (ImGui.BeginListBox("##ImageLogDisplay2", ImGui.GetContentRegionAvail()))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.One);
                Vector2 size = (ImGui.GetContentRegionAvail() - ImGui.GetStyle().ItemSpacing * 2 - Vector2.One * 2) / 3.0f;

                int current = imageOffset * 9;

                int index = 0;

                for (int i = 0; i < 3; i++)
                {
                    for (int f = 0; f < 3; f++)
                    {
                        index++;

                        int indexToUse = index - 1;

                        if (current >= entryCount)
                        {
                            continue;
                        }

                        ISharedImmediateTexture currentTexture = DalamudServices.TextureProvider.GetFromFile(entries[current].ScreenshotPath);

                        IDalamudTextureWrap usedTexture = SearchTexture;

                        if (currentTexture.TryGetWrap(out IDalamudTextureWrap? texture, out Exception? exception))
                        {
                            usedTexture = texture;
                        }

                        if (exception != null)
                        {
                            DalamudServices.PluginLog.Error(exception, "Exception when trying to get texture from file");
                        }

                        Vector2 uv0 = new Vector2(0.15f, 0.15f);
                        Vector2 uv1 = new Vector2(0.85f, 0.85f);

                        uv0 = Vector2.Lerp(uv0, new Vector2(0.01f, 0.01f), animationTimes[indexToUse]);
                        uv1 = Vector2.Lerp(uv1, new Vector2(0.99f, 0.99f), animationTimes[indexToUse]);

                        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 5);
                        ImGui.PushStyleColor(ImGuiCol.Border, 0xFFFFFFFF);

                        if (ImGui.ImageButton(usedTexture.ImGuiHandle, size, uv0, uv1, 0, new Vector4(0, 0.35f, 0.35f, 1)))
                        {
                            selectedIndex = current;
                        }

                        ImGui.PopStyleColor();
                        ImGui.PopStyleVar();

                        animationTimes[indexToUse] -= deltaTime * 2.0f;

                        if (animationTimes[indexToUse] < 0)
                        {
                            animationTimes[indexToUse] = 0;
                        }

                        if (ImGui.IsItemHovered())
                        {
                            animationTimes[indexToUse] += deltaTime * 4.0f;

                            if (animationTimes[indexToUse] > 1)
                            {
                                animationTimes[indexToUse] = 1;
                            }
                        }

                        current++;

                        if (f != 2)
                        {
                            ImGui.SameLine();
                        }
                    }
                }
                ImGui.PopStyleVar();

                ImGui.EndListBox();
            }

            ImGui.EndListBox();
        }

        ImGui.SameLine();

        if (ImGui.BeginListBox("##ImageExplainBox", ImGui.GetContentRegionAvail()))
        {
            if (selectedIndex >= 0)
            {
                ISharedImmediateTexture currentTexture = DalamudServices.TextureProvider.GetFromFile(entries[selectedIndex].ScreenshotPath);

                IDalamudTextureWrap usedTexture = SearchTexture;

                if (currentTexture.TryGetWrap(out IDalamudTextureWrap? texture, out Exception? exception))
                {
                    usedTexture = texture;
                }
                
                if (exception != null)
                {
                    DalamudServices.PluginLog.Error(exception, "Exception when trying to get texture from file");
                }

                float width  = usedTexture.Width;
                float height = usedTexture.Height;

                float resolution = height / width;

                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
                ImGui.PushStyleColor(ImGuiCol.Border, 0xFFFFFFFF);

                float widthAvail = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X;

                if (ImGui.ImageButton(usedTexture.ImGuiHandle, new Vector2(widthAvail, widthAvail * resolution)))
                {
                    fullScreenMode = true;
                }

                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
            }

            ImGui.EndListBox();
        }
    }

    public void UpdateImageOffset(int offsetDirection, int pageCount)
    {
        imageOffset += offsetDirection;

        if (imageOffset < 0)
        {
            imageOffset = 0;
        }

        if (imageOffset > pageCount)
        {
            imageOffset = pageCount;
        }
    }

    public void Dispose()
    {
        SearchTexture?.Dispose();
        DateBar?.Dispose();
    }
}
