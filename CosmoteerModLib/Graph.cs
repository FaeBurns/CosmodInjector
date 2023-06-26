namespace CosmoteerModLib;

public class Graph<T>
{
    private readonly List<Node> _nodes = new List<Node>();

    public Node Add(T data)
    {
        Node node = new Node(data, _nodes.Count);
        _nodes.Add(node);
        return node;
    }

    public void ConnectTo(Node a, Node b, ConnectionType connectionType)
    {
        switch (connectionType)
        {
            case ConnectionType.OneWay:
                ConnectSingle(a, b);
                break;
            case ConnectionType.TwoWay:
                ConnectSingle(a, b);
                ConnectSingle(b, a);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
        }
    }

    private void ConnectSingle(Node a, Node b)
    {
        a.OutgoingConnections.Add(b);
        b.IncomingConnections.Add(a);
    }

    /// <summary>
    /// Gets a list of all nodes contained in a cycle.
    /// </summary>
    /// <returns>(Node, Children of node in the cycle)</returns>
    public List<(Node, List<Node>)> GetCycles()
    {
        HashSet<Node> seenNodes = new HashSet<Node>();
        List<(Node, List<Node>)> cyclicNodes = new List<(Node, List<Node>)>();

        foreach (Graph<T>.Node node in _nodes)
        {
            // if this node has been seen before in the search then skip
            if (seenNodes.Contains(node))
                continue;

            bool[] visited = new bool[_nodes.Count];
            bool[] searching = new bool[_nodes.Count];

            if (IsCyclicRecursive(node, visited, searching, seenNodes))
            {
                for (int i = 0; i < searching.Length; i++)
                {
                    bool inCycle = searching[i];
                    if (inCycle)
                        cyclicNodes.Add((_nodes[i], _nodes[i].OutgoingConnections.Where(n => searching[n.Index]).ToList()));
                }
            }
        }
        return cyclicNodes;
    }

    private bool IsCyclicRecursive(Node node, bool[] visited, bool[] searching, HashSet<Node> seenNodes)
    {
        if (searching[node.Index])
            return true;

        if (visited[node.Index])
            return false;

        visited[node.Index] = true;
        searching[node.Index] = true;
        seenNodes.Add(_nodes[node.Index]);

        foreach (Node child in _nodes[node.Index].OutgoingConnections)
            if (IsCyclicRecursive(child, visited, searching, seenNodes))
                return true;

        searching[node.Index] = false;
        return false;
    }

    public class Node
    {
        public T Data { get; }

        internal List<Node> OutgoingConnections { get; } = new List<Node>();
        internal List<Node> IncomingConnections { get; } = new List<Node>();

        internal int Index { get; }

        internal Node(T data, int index)
        {
            Data = data;
            Index = index;
        }

        public override string ToString()
        {
            return Data!.ToString()!;
        }
    }
}

public enum ConnectionType
{
    OneWay,
    TwoWay,
}