namespace Configurator;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        DialogResult dialogResult = folderBrowser.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            File.WriteAllText("game.txt", folderBrowser.SelectedPath);
        }
    }
}