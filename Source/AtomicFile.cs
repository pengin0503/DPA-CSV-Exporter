using System;
using System.IO;
using System.Text;

namespace DpaCsvExporter;

/// <summary>Expose a completed CSV only after the entire write succeeds.</summary>
public static class AtomicFile
{
    public static void Write(string destination, Action<TextWriter> write)
    {
        if (write == null) throw new ArgumentNullException(nameof(write));
        var tempPath = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var writer = new StreamWriter(tempPath, false, new UTF8Encoding(true)))
                write(writer);
            File.Move(tempPath, destination);
        }
        catch
        {
            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch (IOException) { /* preserve the original error */ }
            catch (UnauthorizedAccessException) { /* preserve the original error */ }
            throw;
        }
    }
}
