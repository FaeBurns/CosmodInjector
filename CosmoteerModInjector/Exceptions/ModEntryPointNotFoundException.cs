using System.Reflection;

namespace CosmoteerModInjector.Exceptions;

public class ModEntryPointNotFoundException : Exception
{
    public Assembly Assembly { get; }

    public ModEntryPointNotFoundException(Assembly assembly)
    {
        Assembly = assembly;
    }
}