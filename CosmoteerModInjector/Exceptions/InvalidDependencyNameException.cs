using CosmoteerModLib;

namespace CosmoteerModInjector.Exceptions;

public class InvalidDependencyNameException : Exception
{
    public ModInfo Mod { get; }
    public string Dependency { get; }

    public InvalidDependencyNameException(ModInfo mod, string dependency)
    {
        Mod = mod;
        Dependency = dependency;
    }
}