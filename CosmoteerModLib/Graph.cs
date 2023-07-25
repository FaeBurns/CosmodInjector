namespace CosmoteerModLib;

public class Graph<T>
{
    private readonly List<Node> m_nodes = new List<Node>();

    public Node Add(T data)
    {
        Node node = new Node(data, m_nodes.Count);
        m_nodes.Add(node);
        return node;
    }

    public void ConnectTo(Node a, Node b, ConnectionType connectionType)
    {
        switch (connectionType)
        {
            case ConnectionType.ONE_WAY:
                ConnectSingle(a, b);
                break;
            case ConnectionType.TWO_WAY:
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

        foreach (Graph<T>.Node node in m_nodes)
        {
            // if this node has been seen before in the search then skip
            if (seenNodes.Contains(node))
                continue;

            bool[] visited = new bool[m_nodes.Count];
            bool[] searching = new bool[m_nodes.Count];

            if (IsCyclicRecursive(node, visited, searching, seenNodes))
            {
                for (int i = 0; i < searching.Length; i++)
                {
                    bool inCycle = searching[i];
                    if (inCycle)
                        cyclicNodes.Add((m_nodes[i], m_nodes[i].OutgoingConnections.Where(n => searching[n.Index]).ToList()));
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
        seenNodes.Add(m_nodes[node.Index]);

        foreach (Node child in m_nodes[node.Index].OutgoingConnections)
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
    ONE_WAY,
    TWO_WAY,
}