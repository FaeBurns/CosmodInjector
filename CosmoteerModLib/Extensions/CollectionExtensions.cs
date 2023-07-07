using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CosmoteerModLib.Extensions;

public static class CollectionExtensions
{
    public static bool ContainsAll<T>(this HashSet<T> collection, IEnumerable<T> targets)
    {
        foreach (T target in targets)
        {
            if (!collection.Contains(target))
                return false;
        }

        return true;
    }

    public static bool ContainsType<T>(this IEnumerable collection)
    {
        foreach (object item in collection)
        {
            if (item.GetType() == typeof(T))
                return true;
        }

        return false;
    }

    public static bool TryGetInstanceOfType<T>(this IEnumerable collection, out T result)
    {
        foreach (object item in collection)
        {
            if (item.GetType() == typeof(T))
            {
                result = (T)item;
                return true;
            }
        }

        result = default(T)!;
        return false;
    }
}