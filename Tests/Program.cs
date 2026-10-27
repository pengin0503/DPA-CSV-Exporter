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
Console.WriteLine("CSV precision and atomic file regression tests passed");
