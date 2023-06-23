using System.Reflection;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

public class TeximpNetImportPatch : IPatch
{
    public void Patch(Harmony harmony)
    {
        const string patchingType = "TeximpNet.Unmanaged.UnmanagedLibrary";
        const string patchingMethod = "NativeLoadLibrary";

        // would be loaded later by game anyway
        Assembly targetAssembly = Assembly.LoadFrom("TeximpNet.dll");

        Type? type = targetAssembly.GetType(patchingType)!;
        Type nestedType = type.GetNestedType("UnmanagedWindowsLibraryImplementation", BindingFlags.NonPublic)!;
        MethodInfo? method = nestedType.GetMethod(patchingMethod, (BindingFlags)Int32.MaxValue, new[]{typeof(string)});

        if (method == null)
            throw new NullReferenceException($"method for {patchingMethod} was null");

        harmony.Patch(method, new HarmonyMethod(Prefix));
    }

    private static void Prefix(ref string path)
    {
        path = "Bin/" + path;
    }
}