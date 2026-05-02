using System;
using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using XivFatalFrame.Services;

namespace XivFatalFrame.Screenshotter;

/// <summary>
/// Helper class that helps me find the new screenshot key when it has changed in a second!
/// </summary>
internal unsafe class ScreenshotKeyFinder : IDisposable
{
#if DEBUG
    private readonly DalamudServices DalamudServices;
    private readonly Configuration   Configuration;
    
    private delegate byte IsInputIdClickedDelegate(UIInputData* uiInputData, int key);
    
    [Signature("E9 ?? ?? ?? ?? 83 7F ?? ?? 0F 8F ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8B CB", DetourName = nameof(IsInputIdClickedDetour))]
    private readonly Hook<IsInputIdClickedDelegate>? IsInputIdClickedHook = null;
    
    private readonly Dictionary<int, byte> KeyStates = [];
    
    public ScreenshotKeyFinder(DalamudServices dalamudServices, Configuration configuration)
    {
        DalamudServices = dalamudServices;
        Configuration   = configuration;
        
        DalamudServices.Hooking.InitializeFromAttributes(this);
        
        IsInputIdClickedHook?.Enable();
    }
    
    private byte IsInputIdClickedDetour(UIInputData* uiInputData, int key)
    {
        byte output = IsInputIdClickedHook!.Original(uiInputData, key);
     
        if (!KeyStates.TryAdd(key, output))
        {
            if (KeyStates[key] != output)
            {
                if (!Configuration.DebugSilenceKeyLog)
                {
                    DalamudServices.PluginLog.Verbose($"Found Key: [{key}. {output}]");
                }
            }
        }
        
        return output;
    }
#endif
    public void Dispose()
    {
        #if DEBUG
        IsInputIdClickedHook?.Dispose();
#endif
    }
}
