using DpaCsvExporter;
static void Equal(string want, string got) { if (want != got) throw new Exception($"Expected <{want}> got <{got}>"); }
Equal("plain", CsvFormat.Escape("plain"));
Equal("\"a,b\"", CsvFormat.Escape("a,b"));
Equal("\"a\"\"b\"", CsvFormat.Escape("a\"b"));
Equal("\"a\r\nb\"", CsvFormat.Escape("a\r\nb"));
Equal("'=SUM(A1)", CsvFormat.Escape("=SUM(A1)"));
Equal("'+cmd", CsvFormat.Escape("+cmd"));
Equal("'@formula", CsvFormat.Escape("@formula"));
Equal("'  =SUM(A1)", CsvFormat.Escape("  =SUM(A1)"));
Equal("0.25", CsvFormat.Number(0.25));
Equal("", CsvFormat.Number(double.NaN));
Equal("\"値, 値\",3.5", CsvFormat.Line(new string?[] { "値, 値", "3.5" }, new HashSet<int> { 1 }));
// Regression: tiny timing values must round-trip rather than disappear at 9 decimal places.
foreach (double n in new[] { 1e-10, -1e-12, Math.PI, 0.00000000012345678, -123456789.12345678 })
{
    var text = CsvFormat.Number(n);
    var parsed = double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    Equal(n.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
          parsed.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
}
// A failed export must not leave a final *.csv file.
var exportDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dpa-csv-test-" + Guid.NewGuid().ToString("N"));
System.IO.Directory.CreateDirectory(exportDir);
try
{
    var finalPath = System.IO.Path.Combine(exportDir, "result.csv");
    bool threw = false;
    try
    {
        AtomicFile.Write(finalPath, writer =>
        {
            writer.WriteLine("incomplete");
            throw new System.IO.IOException("simulated write failure");
        });
    }
    catch (System.IO.IOException) { threw = true; }
    if (!threw || System.IO.File.Exists(finalPath))
        throw new Exception("Atomic export should fail without leaving an incomplete result");
    if (System.IO.Directory.GetFiles(exportDir).Length != 0)
        throw new Exception("Atomic export should remove failed temp file");

    AtomicFile.Write(finalPath, writer => writer.WriteLine("complete"));
    Equal("complete", System.IO.File.ReadAllText(finalPath).TrimStart('\uFEFF').Trim());
}
finally
{
    System.IO.Directory.Delete(exportDir, true);
}
// Validate all fields and rows of the full CSV document independently of the game.
if (CsvReport.Columns.Length != 29) throw new Exception("Expected exactly 29 CSV columns");
var first = Enumerable.Repeat<string?>("", 29).ToArray();
var second = Enumerable.Repeat<string?>("", 29).ToArray();
first[12] = "method,with,commas"; first[18] = "1.5"; first[10] = "1";
second[12] = "another"; second[18] = "2.5"; second[10] = "2";
using (var sw = new System.IO.StringWriter())
{
    CsvReport.Write(sw, new[] { first, second });
    var lines = sw.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    if (lines.Length != 3) throw new Exception("Header plus 2 measurement rows expected");
    if (!lines[0].StartsWith("timestamp_utc,rimworld_version,category,"))
        throw new Exception("Unexpected report header");
    if (!lines[1].Contains("\"method,with,commas\"") || !lines[2].Contains("another"))
        throw new Exception("Report must retain every row and quote text correctly");
}
Console.WriteLine("CSV precision, atomic file and full document tests passed");

// Paired export must publish both completed files or roll back its own files.
var pairDir = Path.Combine(Path.GetTempPath(), "dpa-pair-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(pairDir);
var csvPath = Path.Combine(pairDir, "sample.csv");
var jsonPath = Path.Combine(pairDir, "sample.json");
void WritePair(Action<TextWriter> csv, Action<TextWriter> json)
{
    AtomicFile.WritePair(csvPath, csv, jsonPath, json);
}
void PairMustFail(Action<TextWriter> csv, Action<TextWriter> json)
{
    bool failed = false;
    try { WritePair(csv, json); } catch (IOException) { failed = true; }
    if (!failed) throw new Exception("Injected pair write failure must surface");
    if (Directory.GetFiles(pairDir, "*.tmp").Length != 0) throw new Exception("Pair must remove failed staging files");
}
try
{
    PairMustFail(w => { w.Write("partial"); throw new IOException("CSV failure"); }, w => w.Write("{}"));
    if (File.Exists(csvPath) || File.Exists(jsonPath)) throw new Exception("CSV failure must leave no final files");
    PairMustFail(w => w.Write("complete"), w => { w.Write("partial"); throw new IOException("JSON failure"); });
    if (File.Exists(csvPath) || File.Exists(jsonPath)) throw new Exception("JSON failure must leave no orphan CSV");
    File.WriteAllText(csvPath, "old CSV");
    PairMustFail(w => w.Write("new CSV"), w => w.Write("{}"));
    Equal("old CSV", File.ReadAllText(csvPath));
    if (File.Exists(jsonPath)) throw new Exception("Failed CSV publication must roll back new JSON");
    File.Delete(csvPath);
    File.WriteAllText(jsonPath, "old JSON");
    PairMustFail(w => w.Write("new CSV"), w => w.Write("{}"));
    Equal("old JSON", File.ReadAllText(jsonPath));
    if (File.Exists(csvPath)) throw new Exception("JSON collision must not publish CSV");
    File.Delete(jsonPath);
    WritePair(w => w.Write("complete CSV"), w => w.Write("{}"));
    Equal("complete CSV", File.ReadAllText(csvPath));
    Equal("{}", File.ReadAllText(jsonPath));
    if (Directory.GetFiles(pairDir).Length != 2) throw new Exception("Successful pair leaves exactly CSV and JSON");
    Console.WriteLine("Paired export success, failure rollback and pre-existing-file protection passed");
}
finally { Directory.Delete(pairDir, true); }
