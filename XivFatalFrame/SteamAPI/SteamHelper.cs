namespace XivFatalFrame.SteamAPI;

internal unsafe class SteamHelper
{
    private readonly IPluginLog Log;

    public SteamHelper(IPluginLog pluginLog)
    {
        Log = pluginLog;
    }

    public nint GetSteamApiLibraryHandle()
    {
        Framework* framework = GetFramework();
        if (framework == null)
        {
            Log.Debug("Framework was null");
            return nint.Zero;
        }

        return framework->SteamApiLibraryHandle;
    }

    public bool IsSteamInstance()
    {
        Framework* framework = GetFramework();
        if (framework == null)
        {
            Log.Debug("Game is not a Steam game.");
            return false;
        }

        return framework->IsSteamGame;
    }

    public bool IsSteamApiInitialized()
    {
        Framework* framework = GetFramework();
        if (framework == null)
        {
            Log.Debug("Steam API was not initialized.");
            return false;
        }

        return framework->IsSteamApiInitialized();
    }

    public Framework* GetFramework()
    {
        Framework* framework = Framework.Instance();
        if (framework == null)
        {
            Log.Debug("Framework was null");
            return null;
        }

        return framework;
    }
}
