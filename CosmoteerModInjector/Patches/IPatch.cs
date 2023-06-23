using System.Reflection;
using HarmonyLib;

namespace CosmoteerModInjector.Patches;

public interface IPatch
{
    public void Patch(Harmony harmony);
}