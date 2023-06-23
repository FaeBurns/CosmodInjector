using System.Reflection;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

[Disabled]
public class MainPatch : IPatch
{
    public void Patch(Harmony harmony)
    {
        const string patchingType = "Cosmoteer.GameApp";
        const string patchingMethod = "Main";

        Type? type = Assemblies.GameAssembly.GetType(patchingType);
        if (type == null)
            throw new NullReferenceException($"{patchingType} found type was null");
        MethodInfo? method = type.GetMethod(patchingMethod, BindingFlags.Static | BindingFlags.NonPublic);

        harmony.Patch(method, new HarmonyMethod(Prefix));
    }

    private static void Prefix(string[] args)
    {
        MessageBox.Show("prefixed main");
    }
}