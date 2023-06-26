using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using CosmoteerModInjector.Exceptions;
using CosmoteerModLib;
using Newtonsoft.Json;

namespace CosmoteerModInjector;

public static class ModManager
{
    private static readonly List<DiscoveredMod> _discoveredMods = new List<DiscoveredMod>();
    private static readonly Dictionary<ModInfo, Assembly> _modAssemblies = new Dictionary<ModInfo, Assembly>();
    private static readonly ModCollection _loadedMods = new ModCollection();

    public static IReadOnlyList<ModInfo> LoadedMods => _loadedMods;

    public static int ModCount => _modAssemblies.Count;

    public static bool HasLoaded { get; private set; }

    public static void DiscoverMods(IEnumerable<string> paths)
    {
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
                _modAssemblies.Add(mod.ModInfo, loadedMod);
                _loadedMods.Add(mod.ModInfo);
            }
            else
            {
                try
                {
                    Assembly loadedMod = LoadMod(mod);
                    _modAssemblies.Add(mod.ModInfo, loadedMod);
                    _loadedMods.Add(mod.ModInfo);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception trying to load mod {mod.ModInfo.ModName} from {mod.Path}. Exception:\n" + e.Message);
                }
            }
        }

        if (_loadedMods.TrySortInPlace(out List<ModDependencyError> errors))
        {
            // if the dependency sort had no errors, initialize all mods
            InitializeMods();
        }
        else
        {
            StringBuilder errorBuilder = new StringBuilder();
            errorBuilder.AppendLine("Cyclic dependencies detected in mods");
            foreach (ModDependencyError error in errors)
            {
                errorBuilder.AppendLine($"{error.Mod} -> {error.Dependency}");
            }
            errorBuilder.AppendLine("Press OK to continue. No CMI mods will be initialized");
            MessageBox.Show(errorBuilder.ToString(), "Mod sort error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void InitializeMods()
    {
        foreach (ModInfo mod in _loadedMods.Order)
        {
            Assembly modAssembly = _modAssemblies[mod];
            if (Debugger.IsAttached)
            {
                ExecuteEntryPoint(modAssembly);
            }
            else
            {
                try
                {
                    ExecuteEntryPoint(modAssembly);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception trying to initialize mod {mod.ModName}. Exception:\n" + e.Message);
                }
            }
        }

        HasLoaded = true;
    }

    private static Assembly LoadMod(DiscoveredMod mod)
    {
        string entryAssemblyPath = Path.Combine(mod.Path, mod.ModInfo.AssemblyPath);
        if (!entryAssemblyPath.EndsWith(".dll"))
            entryAssemblyPath += ".dll";

        Assembly assembly = Assembly.LoadFrom(entryAssemblyPath);
        return assembly;
    }

    private static void ExecuteEntryPoint(Assembly modAssembly)
    {
        Type? entryPointType = null;
        foreach (Type type in modAssembly.GetTypes())
        {
            if (typeof(IModEntry).IsAssignableFrom(type))
            {
                entryPointType = type;
            }
        }

        if (entryPointType == null)
            throw new ModEntryPointNotFoundException(modAssembly);

        IModEntry entryPoint = (IModEntry)Activator.CreateInstance(entryPointType)!;

        entryPoint.Loaded();
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

        VerifyModInfo(modInfo);

        mod = new DiscoveredMod(modFolder, modInfo);
        return true;
    }

    private static void VerifyModInfo(ModInfo modInfo)
    {
        if (string.IsNullOrWhiteSpace(modInfo.ModName) || string.IsNullOrEmpty(modInfo.ModName))
            throw new InvalidModNameException(modInfo);

        foreach (ModDependencyInfo dependency in modInfo.Dependencies)
        {
            if (string.IsNullOrWhiteSpace(dependency.ModName) || string.IsNullOrEmpty(dependency.ModName))
                throw new InvalidDependencyNameException(modInfo, dependency.ModName);
        }
    }


}