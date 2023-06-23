using Newtonsoft.Json;

namespace CosmoteerModInjector;

#nullable disable
public class ModInfo
{
    [JsonProperty("name", Required = Required.Always)]
    public string ModName;

    [JsonProperty("assembly", Required = Required.Always)]
    public string Assembly;
}