using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using CosmoteerModLib.Exceptions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CosmoteerModLib;

#nullable disable
public class ModInfo
{
    [JsonProperty("name", Required = Required.Always)]
    public string ModName { get; private set; }

    [JsonProperty("assembly", Required = Required.Always)]
    public string AssemblyPath { get; private set; }

    [JsonProperty("dependencies", Required = Required.DisallowNull)]
    private string[] DependencyStrings { get; set; } = Array.Empty<string>();

    [JsonIgnore]
    public ModDependencyInfo[] Dependencies { get; private set; }

    [OnDeserialized]
    [UsedImplicitly]
    private void OnDeserialized(StreamingContext streamingContext)
    {
        Dependencies = DependencyStrings.Select(s => new ModDependencyInfo(s)).ToArray();
    }
}

public partial class ModDependencyInfo
{
    [GeneratedRegex("([A-z0-9]+)[.]([A-z0-9]+)[.]([A-z0-9]+)([?]?)")]
    private static partial Regex GetValidDependencyRegex();

    internal ModDependencyInfo(string dependencyString)
    {
        Match match = GetValidDependencyRegex().Match(dependencyString);

        if (match.Success && match.Length == dependencyString.Length)
        {
            ModName = dependencyString;
            IsOptional = dependencyString.EndsWith('?');
            return;
        }

        throw new ModDependencyParseException(dependencyString);
    }

    public string ModName { get; }
    public bool IsOptional { get; }

    public override string ToString()
    {
        return ModName;
    }
}