using System;
using System.Collections.Generic;
using System.IO;

namespace DpaCsvExporter;

/// <summary>Pure, game-independent CSV document serialization for DPA snapshots.</summary>
public static class CsvReport
{
    public static readonly string[] Columns = {
        "timestamp_utc", "rimworld_version", "category", "entry", "search_filter",
        "fps", "tps", "tps_target", "root_average_ms", "measurement_cycles", "rank",
        "key", "label", "mod", "assembly", "declaring_type", "method",
        "sample_entries", "average_ms_per_cycle", "self_ms_per_cycle", "max_ms_per_cycle",
        "total_ms", "total_calls", "calls_per_cycle", "fraction_of_root", "spike_count",
        "spike_severity", "phase_flags", "pinned"
    };

    private static readonly HashSet<int> Numeric = new() {
        5,6,7,8,9,10,17,18,19,20,21,22,23,24,25,26,27,28
    };

    public static void Write(TextWriter writer, IEnumerable<string?[]> rows)
    {
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (rows == null) throw new ArgumentNullException(nameof(rows));

        writer.WriteLine(CsvFormat.Line(Columns));
        foreach (var row in rows)
        {
            if (row == null || row.Length != Columns.Length)
                throw new InvalidDataException("Invalid number of DPA CSV columns.");
            writer.WriteLine(CsvFormat.Line(row, Numeric));
        }
    }
}
