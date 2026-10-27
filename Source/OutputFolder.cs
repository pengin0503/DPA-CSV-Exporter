using System.Diagnostics;
using System.IO;
using Verse;

namespace DpaCsvExporter;

public static class OutputFolder
{
    public static string Path => System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "AnalyzerExports");

    public static void Open()
    {
        string folder = Path;
        Directory.CreateDirectory(folder);
        // Pass the literal directory to the OS shell; do not build a command line.
        using var process = Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
    }
}
