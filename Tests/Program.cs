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
Console.WriteLine("CSV serialization + round-trip precision tests passed");
