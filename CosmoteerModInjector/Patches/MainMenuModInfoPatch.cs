using System.Reflection;
using Halfling.Gui;
using HarmonyLib;
using Label = Halfling.Gui.Label;

namespace CosmoteerModInjector.Patches;

public class MainMenuModInfoPatch : IPatch
{
    public void Patch(Harmony harmony)
    {
        Type targetType = Assemblies.GameAssembly.GetType("Cosmoteer.Gui.TitleScreen")!;
        ConstructorInfo targetConstructor = targetType.GetConstructor((BindingFlags)Int32.MaxValue, Array.Empty<Type>())!;

        harmony.Patch(targetConstructor, postfix: new HarmonyMethod(Postfix));
    }

    // ReSharper disable once InconsistentNaming
    // name is required
    private static void Postfix(object __instance)
    {
        object rootWidget = __instance.GetType().GetProperties().First(p => p.Name == "RootWidget").GetMethod!.Invoke(__instance, null)!;
        object children = rootWidget.GetType().GetProperty("Children")!.GetMethod!.Invoke(rootWidget, null)!;
        LayoutBox layoutBox = (LayoutBox)children.GetType().GetProperty("Item")!.GetMethod!.Invoke(children, new object[] { 1 })!;
        Label label = (Label)layoutBox.Children[0];
        label.Text += GetModInfoText();
    }

    private static string GetModInfoText()
    {
        int modCount = ModManager.ModCount;
        return $" [CMI]({modCount} mods loaded)";
    }
}