using System.Diagnostics;
using System.Reflection;
using Steamworks;

namespace CosmoteerModInjector;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        if (EnsureSteam())
            return;

        Assemblies.GameAssembly = Assembly.LoadFrom("Cosmoteer.dll");
        Assemblies.EngineAssembly = Assembly.LoadFrom("HalflingCore.dll");
        MethodBase entryPoint = Assemblies.GameAssembly.ManifestModule.ResolveMethod(Assemblies.GameAssembly.EntryPoint!.MetadataToken)!;

        HarmonyPatcher.Patch();
        Logger.Log("Patch complete");

        object[] parameters =
        {
            new string[]
            {
            },
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

        Directory.SetCurrentDirectory(File.ReadAllLines("game.txt")[0]);

        if (Steamworks.SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
        {
            Process.GetCurrentProcess().Kill();
            return true;
        }

        return false;
    }
}