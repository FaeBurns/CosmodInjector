using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CosmoteerModLib;

[assembly: IgnoresAccessChecksTo("Cosmoteer")]
[assembly: IgnoresAccessChecksTo("HalflingCore")]

namespace ModTemplate;

public class Mod : IMod
{
    public void Loaded()
    {
        MessageBox.Show("test mod loaded");
    }
}