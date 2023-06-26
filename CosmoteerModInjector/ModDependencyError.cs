using CosmoteerModLib;

namespace CosmoteerModInjector;

public class ModDependencyError
{
    public ModInfo Mod { get; }
    public string Dependency { get; }
    public ModDependencyErrorReason Type { get; }

    public ModDependencyError(ModInfo mod, string dependency, ModDependencyErrorReason type)
    {
        Mod = mod;
        Dependency = dependency;
        Type = type;
    }

    public enum ModDependencyErrorReason
    {
        Missing,
        CyclicalDependency,
    }
}