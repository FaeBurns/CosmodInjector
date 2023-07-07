using System.Reflection;
using CosmoteerModInjector.Patches;
using HarmonyLib;

namespace CosmoteerModInjector;

internal static class HarmonyPatcher
{
    public static void Patch()
    {
        Harmony harmony = new Harmony("bean.cosmoteer.injector");
        harmony.PatchAll(Assemblies.Self);

        IEnumerable<Type> patchTypes = Assembly.GetExecutingAssembly().GetTypes().Where(p => typeof(IPatch).IsAssignableFrom(p)).ToArray();
        foreach (Type patchType in patchTypes)
        {
            if (patchType == typeof(IPatch))
                continue;

            IPatch patch = (IPatch)Activator.CreateInstance(patchType)!;
            patch.Patch(harmony);
        }
    }
}