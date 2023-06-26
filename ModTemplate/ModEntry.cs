using System.Windows.Forms;
using CosmoteerModLib;

namespace ModTemplate;

public class ModEntry : IModEntry
{
    public void Loaded()
    {
        MessageBox.Show("test mod loaded");
    }
}