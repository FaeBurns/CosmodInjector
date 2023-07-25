using System.Reflection;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

public class RestartOnModChangePatch : IPatch
{
    private static Type s_gameAppType = null!;

    public void Patch(Harmony harmony)
    {
        s_gameAppType = Assemblies.GameAssembly.GetType("Cosmoteer.GameApp")!;
        MethodInfo method = s_gameAppType.GetMethod("RestartApp", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;

        harmony.Patch(method, postfix: new HarmonyMethod(RestartAppPostfix));
    }

    private static void RestartAppPostfix()
    {
        PropertyInfo targetProperty = s_gameAppType.GetProperty("FileToRunOnExit", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        (string, string, string) existingValue = ((string, string, string))targetProperty.GetMethod!.Invoke(null, null)!;

        string exePath = Path.ChangeExtension(Assemblies.Self.Location!, "exe")!;

        targetProperty.SetMethod!.Invoke(null, new object[]
        {
            (exePath, existingValue.Item2, Path.GetDirectoryName(Assemblies.Self.Location)),
        });
    }
}