using System.Diagnostics;
using System.Reflection;
using CosmoteerModLib;
using Steamworks;

namespace CosmoteerModInjector;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // set directory *first* as steam_appid needs to be in the correct place
        string gameBinPath = File.ReadAllLines("game.txt")[0];
        Directory.SetCurrentDirectory(gameBinPath);

        if (EnsureSteam())
            return;

        int sameProcessCount = GetSameProcessCount();
        Logger.Init(Directory.GetParent(gameBinPath)!, sameProcessCount);
        Logger.GetLogger("CMI").Log($"process count: {sameProcessCount}");

        Assemblies.Self = Assembly.GetExecutingAssembly();
        Assemblies.GameAssembly = Assembly.LoadFrom("Cosmoteer.dll");
        Assemblies.EngineAssembly = Assembly.LoadFrom("HalflingCore.dll");
        MethodBase entryPoint = Assemblies.GameAssembly.ManifestModule.ResolveMethod(Assemblies.GameAssembly.EntryPoint!.MetadataToken)!;

        HarmonyPatcher.Patch();
        Logger.GetLogger("CMI").Log("Patch Complete");

        // launch cosmoteer
        object[] parameters =
        {
            args,
        };
        entryPoint.Invoke(null, parameters);
    }

    private static bool EnsureSteam()
    {
        const uint appId = 799600U;

        if (!File.Exists("steam_appid.txt"))
        {
            File.WriteAllText("steam_appid.txt", appId.ToString());
        }

        // ensure game is counted as launched on steam
        if (Steamworks.SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
        {
            Process.GetCurrentProcess().Kill();
            return true;
        }

        return false;
    }

    private static int GetSameProcessCount()
    {
        // - 1 as we want to know how many other processes there are, not including this one
        return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)).Length - 1;
    }
}