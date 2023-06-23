using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CosmoteerModInjector.Exceptions;
using CosmoteerModLib;
using Newtonsoft.Json;

namespace CosmoteerModInjector;

public static class ModManager
{
    private const string UserModsFolderName = "UserMods";

    private static readonly List<DiscoveredMod> _discoveredMods = new List<DiscoveredMod>();
    private static readonly Dictionary<ModInfo, Assembly> _loadedMods = new Dictionary<ModInfo, Assembly>();

    public static IReadOnlyDictionary<ModInfo, Assembly> LoadedMods => _loadedMods;
    public static int ModCount => _loadedMods.Count;

    public static bool HasLoaded { get; private set; }

    public static void FetchMods(IEnumerable<string> paths)
    {
        _discoveredMods.AddRange(FetchUserMods());

        foreach (string path in paths)
        {
            if (TryDiscoverMod(path, out DiscoveredMod? mod))
            {
                _discoveredMods.Add(mod);
            }
        }

        foreach (DiscoveredMod mod in _discoveredMods)
        {
            Logger.Log($"Discovered mod {mod}");
        }
    }

    public static void LoadMods()
    {
        foreach (DiscoveredMod mod in _discoveredMods)
        {
            if (Debugger.IsAttached)
            {
                Assembly loadedMod = LoadMod(mod);
                _loadedMods.Add(mod.ModInfo, loadedMod);
            }
            else
            {
                try
                {
                    Assembly loadedMod = LoadMod(mod);
                    _loadedMods.Add(mod.ModInfo, loadedMod);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception trying to load mod {mod.ModInfo.ModName} from {mod.Path}. Exception:\n" + e.Message);
                }
            }
        }

        HasLoaded = true;
    }

    public static void InitializeMods()
    {
        foreach (KeyValuePair<ModInfo, Assembly> mod in _loadedMods)
        {
            if (Debugger.IsAttached)
            {
                ExecuteEntryPoint(mod.Value);
            }
            else
            {
                try
                {
                    ExecuteEntryPoint(mod.Value);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception trying to initialize mod {mod.Key.ModName}. Exception:\n" + e.Message);
                }
            }
        }
    }

    private static Assembly LoadMod(DiscoveredMod mod)
    {
        string entryAssemblyPath = Path.Combine(mod.Path, mod.ModInfo.Assembly);
        Assembly assembly = Assembly.LoadFrom(entryAssemblyPath);
        return assembly;
    }

    private static void ExecuteEntryPoint(Assembly modAssembly)
    {
        Type? entryPointType = null;
        foreach (Type type in modAssembly.GetTypes())
        {
            if (typeof(IMod).IsAssignableFrom(type))
            {
                entryPointType = type;
            }
        }

        if (entryPointType == null)
            throw new ModEntryPointNotFoundException(modAssembly);

        IMod entryPoint = (IMod)Activator.CreateInstance(entryPointType)!;

        entryPoint.Loaded();
    }

    private static IEnumerable<DiscoveredMod> FetchUserMods()
    {
        Directory.CreateDirectory(UserModsFolderName);
        return GetModsInFolder(UserModsFolderName);
    }

    private static IEnumerable<DiscoveredMod> GetModsInFolder(string folderPath)
    {
        List<DiscoveredMod> result = new List<DiscoveredMod>();
        string[] potentialModDirectories = Directory.GetDirectories(folderPath);
        foreach (string modDirectory in potentialModDirectories)
        {
            if (TryDiscoverMod(modDirectory, out DiscoveredMod? mod))
            {
                result.Add(mod);
            }
        }
        return result;
    }

    private static bool TryDiscoverMod(string modFolder, [NotNullWhen(true)] out DiscoveredMod? mod)
    {
        string modInfoFile = Path.Combine(modFolder, "mod.json");
        if (!File.Exists(modInfoFile))
        {
            mod = null;
            return false;
        }

        ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoFile))!;
        mod = new DiscoveredMod(modFolder, modInfo);
        return true;
    }
}