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
}