namespace CosmoteerModLib.Exceptions;

public class ModDependencyParseException : Exception
{
    public string DependencyString { get; }

    public ModDependencyParseException(string dependencyString)
    {
        DependencyString = dependencyString;
    }
}