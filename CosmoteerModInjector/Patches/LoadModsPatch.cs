using System.Collections;
using System.Reflection;
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

        AbsolutePath[] enabledModPaths = GetEnabledModPaths().ToArray();
        IEnumerable<string> paths = enabledModPaths.Select(p => p.ToString());
        ModManager.FetchMods(paths);
        ModManager.LoadMods();
        ModManager.InitializeMods();
        return result;
    }

    private static HashSet<AbsolutePath> GetEnabledModPaths()
    {
        Type settingsType = Assemblies.GameAssembly.GetType("Cosmoteer.Settings")!;
        PropertyInfo enabledModsProperty = settingsType.GetProperty("EnabledMods", BindingFlags.Static | BindingFlags.Public)!;
        return (HashSet<AbsolutePath>)enabledModsProperty.GetMethod!.Invoke(null, null)!;
    }
}