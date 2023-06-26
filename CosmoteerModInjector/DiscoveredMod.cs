#nullable disable
using CosmoteerModLib;

namespace CosmoteerModInjector;

public class DiscoveredMod
{
    public string Path;
    public ModInfo ModInfo;

    public DiscoveredMod(string path, ModInfo modInfo)
    {
        Path = path;
        ModInfo = modInfo;
    }
}