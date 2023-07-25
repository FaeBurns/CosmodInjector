using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CosmoteerModInjector.Exceptions;
using CosmoteerModLib;
using Newtonsoft.Json;

namespace CosmoteerModInjector;

public static class ModManager
{
    private static readonly List<DiscoveredMod> s_discoveredMods = new List<DiscoveredMod>();
    private static readonly Dictionary<ModInfo, Assembly> s_modAssemblies = new Dictionary<ModInfo, Assembly>();
    private static readonly ModCollection s_loadedMods = new ModCollection();

    public static IReadOnlyList<ModInfo> LoadedMods => s_loadedMods;

    public static int ModCount => s_modAssemblies.Count;

    public static bool HasLoaded { get; private set; }

    public static void DiscoverMods(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            if (TryDiscoverMod(path, out DiscoveredMod? mod))
            {
                s_discoveredMods.Add(mod);
            }
        }

        foreach (DiscoveredMod mod in s_discoveredMods)
        {
            Logger.GetLogger("CMI").Log($"Discovered mod {mod}");
        }
    }

    public static void LoadMods()
    {
        foreach (DiscoveredMod mod in s_discoveredMods)
        {
            Assembly loadedMod;
            try
            {
                loadedMod = LoadMod(mod);
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                
                MessageBox.Show($"Exception trying to load mod {mod.ModInfo.ModName} from {mod.Path}. Exception:\n" + e.Message);
                continue;
            }
            s_modAssemblies.Add(mod.ModInfo, loadedMod);
            s_loadedMods.Add(mod.ModInfo);
        }

        if (s_loadedMods.TrySortInPlace(out List<ModDependencyError> errors))
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
        foreach (ModInfo mod in s_loadedMods.Order)
        {
            Assembly modAssembly = s_modAssemblies[mod];
            
            try
            {
                ExecuteEntryPoint(modAssembly);
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                
                MessageBox.Show($"Exception trying to initialize mod {mod.ModName}. Exception:\n" + e.Message);
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