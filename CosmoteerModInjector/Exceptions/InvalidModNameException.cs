using CosmoteerModLib;

namespace CosmoteerModInjector.Exceptions;

public class InvalidModNameException : Exception
{
    public ModInfo ModInfo { get; }

    public InvalidModNameException(ModInfo modInfo)
    {
        ModInfo = modInfo;
    }
}