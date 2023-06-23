using System.Reflection;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

public class SteamPatch : IPatch
{
    public void Patch(Harmony harmony)
    {
        Type? type = Assemblies.GameAssembly.GetType("Cosmoteer.Steamworks.Steam");
        if (type == null)
            throw new NullReferenceException("Cosmoteer.Steamworks.Steam found type was null");
        MethodInfo? method = type.GetMethod("RestartAppIfNecessary", BindingFlags.Static | BindingFlags.Public);

        //harmony.Patch(method, new HarmonyMethod(Prefix));
    }

    private static bool Prefix()
    {
        return false;
    }
}