using System.Reflection;

namespace CosmoteerModInjector.Exceptions;

public class ModEntryPointNotFoundException : Exception
{
    public ModEntryPointNotFoundException(Assembly assembly)
    {
    }
}