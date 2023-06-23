namespace CosmoteerModLib;

/// <summary>
/// An interface defining the entry point of a mod.
/// </summary>
public interface IMod
{
    /// <summary>
    /// Called when the mod is loaded.
    /// </summary>
    public void Loaded();
}