using System.Collections;
using System.Diagnostics;
using CosmoteerModLib;
using CosmoteerModLib.Extensions;

namespace CosmoteerModInjector;

public class ModCollection : IReadOnlyList<ModInfo>
{
    private List<ModInfo> m_mods = new List<ModInfo>();

    public IReadOnlyList<ModInfo> Order => m_mods;

    public void AddRange(IEnumerable<ModInfo> modInfos)
    {
        foreach (ModInfo modInfo in modInfos)
        {
            Add(modInfo);
        }
    }

    public void Add(ModInfo modInfo)
    {
        m_mods.Add(modInfo);
    }

    public void Remove(ModInfo modInfo)
    {
        m_mods.Remove(modInfo);
    }

    /// <summary>
    /// Tries to sort this mod collection. Only applies changes if no dependency errors occur.
    /// </summary>
    /// <param name="errors">All errors that occured during the sort.</param>
    /// <returns>True if the sort was successful, false if not</returns>
    public bool TrySortInPlace(out List<ModDependencyError> errors)
    {
        (List<ModInfo> mods, List<ModDependencyError> fulfillmentErrors) = TrySortDependencies(m_mods);
        errors = fulfillmentErrors;
        if (fulfillmentErrors.Count == 0)
            m_mods = mods;

        return errors.Count == 0;
    }

    /// <summary>
    /// Sorts a collection of mods into the correct initialization order for their defined dependencies.
    /// </summary>
    /// <param name="mods">The mods to sort.</param>
    /// <returns><p>A list of mod names in the desired order and a list of any errors that occured during sorting. the first list will not include any mods that resulted in errors during loading.</p>
    /// <p>When the second list contains items, the first list's contents should be treated as undefined.</p></returns>
    public (List<ModInfo> mods, List<ModDependencyError> fulfillmentErrors) TrySortDependencies(IReadOnlyList<ModInfo> mods)
    {
        List<ModInfo> modOrder = new List<ModInfo>();
        List<ModDependencyError> dependencyErrors = new List<ModDependencyError>();

        // keep lookup of mods that have been ordered
        HashSet<string> committedModsSet = new HashSet<string>();

        Dictionary<string, ModInfo> modNameMapping = mods.ToDictionary(m => m.ModName, m => m);
        List<ModInfo> modsWaitingForDependencies = new List<ModInfo>();

        foreach (ModInfo modInfo in mods)
        {
            // if mod has 0 dependencies, don't bother changing order, add to result at current location
            if (modInfo.Dependencies.Length == 0)
            {
                committedModsSet.Add(modInfo.ModName);
                modOrder.Add(modInfo);
                continue;
            }

            if (CheckDependenciesPresent(modInfo, modNameMapping, dependencyErrors))
            {
                // if all dependencies are present in mods list
                // check if all of them are currently loaded
                // if so then add this mod to the result and move on to the next
                if (committedModsSet.ContainsAll(modInfo.Dependencies.Select(d => d.ModName)))
                {
                    committedModsSet.Add(modInfo.ModName);
                    modOrder.Add(modInfo);
                }
                else
                    modsWaitingForDependencies.Add(modInfo);
            }
        }

        // if there are no mods waiting then we can exit early here.
        if (modsWaitingForDependencies.Count == 0)
            return (modOrder, dependencyErrors);

        // create the dependency graph - will be used to check for cycles
        Graph<ModInfo> dependencyGraph = new Graph<ModInfo>();
        // mapping of mod names to nodes - used to find the node of a dependency
        Dictionary<string, Graph<ModInfo>.Node> nodeMap = new Dictionary<string, Graph<ModInfo>.Node>();

        // populate the graph with nodes
        foreach (ModInfo mod in mods)
        {
            Graph<ModInfo>.Node node = dependencyGraph.Add(mod);
            nodeMap.Add(mod.ModName, node);
        }

        // now create connections between nodes and their dependencies
        // the two step process is because all nodes need to be present first before creating connections
        // assumes that all dependencies have a node, which they should at this point
        foreach (ModInfo mod in mods)
        {
            Graph<ModInfo>.Node modNode = nodeMap[mod.ModName];

            foreach (ModDependencyInfo dependency in mod.Dependencies)
            {
                dependencyGraph.ConnectTo(modNode, nodeMap[dependency.ModName], ConnectionType.ONE_WAY);
            }
        }

        // get all cycles found in the graph and add an error for each mod-dependency in the cycle
        List<(Graph<ModInfo>.Node, List<Graph<ModInfo>.Node>)> cycles = dependencyGraph.GetCycles();
        foreach ((Graph<ModInfo>.Node node, List<Graph<ModInfo>.Node> cyclicChildren) in cycles)
        {
            foreach (Graph<ModInfo>.Node child in cyclicChildren)
            {
                dependencyErrors.Add(new ModDependencyError(node.Data, child.Data.ModName, ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
            }
        }

        // if there were any cycles found, exit early
        if (cycles.Count > 0)
            return (modOrder, dependencyErrors);

        // assert that there are no duplicate mods in the load order
        Debug.Assert(new HashSet<ModInfo>(mods).Count == mods.Count);

        // otherwise add the remaining mods to the load order
        // AddDependenciesRecursive adds from the bottom up so mods will be in the correct order
        foreach (ModInfo mod in modsWaitingForDependencies)
        {
            AddDependenciesRecursive(nodeMap[mod.ModName], modOrder, committedModsSet);
        }

        // final return with everything ok
        return (modOrder, dependencyErrors);
    }

    /// <summary>
    /// Recursively traverses the graph to add dependencies in their required load order.
    /// </summary>
    /// <param name="currentNode">The node to start with.</param>
    /// <param name="result">The ordered collection to place the mods in.</param>
    /// <param name="alreadyAdded">A lookup of all mods that have already been sorted.</param>
    private void AddDependenciesRecursive(Graph<ModInfo>.Node currentNode, List<ModInfo> result, HashSet<string> alreadyAdded)
    {
        if (alreadyAdded.Contains(currentNode.Data.ModName))
            return;

        foreach (Graph<ModInfo>.Node childNode in currentNode.OutgoingConnections)
        {
            AddDependenciesRecursive(childNode, result, alreadyAdded);
        }

        alreadyAdded.Add(currentNode.Data.ModName);
        result.Add(currentNode.Data);
    }

    private static bool CheckDependenciesPresent(ModInfo modInfo, IReadOnlyDictionary<string, ModInfo> mods, List<ModDependencyError> fulfillmentErrors)
    {
        bool allDependenciesPresent = true;

        // loop through all dependencies
        foreach (ModDependencyInfo dependency in modInfo.Dependencies)
        {
            // if dependency has not/will not be loaded
            if (!mods.ContainsKey(dependency.ModName))
            {
                // first check if it's optional
                // if it is then do nothing, values are already fine
                // if it is not, then add the error and set the flag
                if (!dependency.IsOptional)
                {
                    fulfillmentErrors.Add(new ModDependencyError(modInfo, dependency.ModName, ModDependencyError.ModDependencyErrorReason.MISSING));
                    allDependenciesPresent = false;
                }
            }
        }

        return allDependenciesPresent;
    }

    public IEnumerator<ModInfo> GetEnumerator()
    {
        return m_mods.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => m_mods.Count;

    public ModInfo this[int index] => m_mods[index];
}