using System;
using System.IO;
using System.Text;

namespace DpaCsvExporter;

/// <summary>Expose a completed CSV only after the entire write succeeds.</summary>
public static class AtomicFile
{
    /// <summary>Stage both files, publish JSON before CSV, and undo only our own files on failure.</summary>
    public static void WritePair(string csvPath, Action<TextWriter> writeCsv,
        string jsonPath, Action<TextWriter> writeJson)
    {
        if (writeCsv == null) throw new ArgumentNullException(nameof(writeCsv));
        if (writeJson == null) throw new ArgumentNullException(nameof(writeJson));
        string csvTemp = csvPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        string jsonTemp = jsonPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        bool jsonPublished = false, csvPublished = false;
        try
        {
            using (var writer = new StreamWriter(csvTemp, false, new UTF8Encoding(true))) writeCsv(writer);
            using (var writer = new StreamWriter(jsonTemp, false, new UTF8Encoding(false))) writeJson(writer);
            File.Move(jsonTemp, jsonPath);
            jsonPublished = true;
            File.Move(csvTemp, csvPath);
            csvPublished = true;
        }
        catch
        {
            if (csvPublished) TryDelete(csvPath);
            if (jsonPublished) TryDelete(jsonPath);
            throw;
        }
        finally
        {
            TryDelete(csvTemp);
            TryDelete(jsonTemp);
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (IOException) { /* preserve the original error */ }
        catch (UnauthorizedAccessException) { /* preserve the original error */ }
    }

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
