using System.Collections;
using System.Reflection;
using CosmoteerModLib;
using Halfling;
using Halfling.IO;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

public class LoadModsPatch : IPatch
{
    public void Patch(Harmony harmony)
    {
        const string patchingType = "Cosmoteer.Mods.ModInfo";
        const string patchingMethod = "LoadMods";

        Type? type = Assemblies.GameAssembly.GetType(patchingType);
        if (type == null)
            throw new NullReferenceException($"{patchingType} found type was null");
        MethodInfo? method = type.GetMethod(patchingMethod, BindingFlags.Static | BindingFlags.Public);

        harmony.Patch(method, postfix: new HarmonyMethod(Postfix));
    }

    public static IList Postfix(IList result)
    {
        if (ModManager.HasLoaded)
            return result;

        // now is a good time to hook the logger
        App.Director.FrameEnded += (_, _) => Logger.FlushAll(Logger.LogFlushMode.SingleFile);

        // force add UserMods as they should always be enabled
        Directory.CreateDirectory("UserMods");
        ModManager.DiscoverMods(Directory.GetDirectories("UserMods"));

        // get the paths of all enabled mods
        AbsolutePath[] enabledModPaths = GetEnabledModPaths().ToArray();
        IEnumerable<string> modPaths = enabledModPaths.Select(p => p.ToString());

        // discover mods in specified path
        ModManager.DiscoverMods(modPaths);

        // perform load
        ModManager.LoadMods();
        return result;
    }

    private static HashSet<AbsolutePath> GetEnabledModPaths()
    {
        Type settingsType = Assemblies.GameAssembly.GetType("Cosmoteer.Settings")!;
        PropertyInfo enabledModsProperty = settingsType.GetProperty("EnabledMods", BindingFlags.Static | BindingFlags.Public)!;
        return (HashSet<AbsolutePath>)enabledModsProperty.GetMethod!.Invoke(null, null)!;
    }
}