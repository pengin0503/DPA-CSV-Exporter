using Analyzer;
using DpaCsvExporter;
using UnityEngine;
using Verse;
static void Check(bool ok, string reason) { if (!ok) throw new Exception(reason); }
_ = new DpaCsvExporterMod(new ModContentPack());
var window = new Window_Analyzer();
// A tab lies beneath the old painted button. The selected entry must survive export.
Widgets.MouseX = 50; Widgets.MouseY = 665; Widgets.Click = true;
window.DoWindowContents(new Rect(0, 0, 1000, 700));
Check(Exporter.Exports == 1, "Export click was stolen by the underlying DPA entry");
Check(Window_Analyzer.Entry == "Tick" && Exporter.ExportedEntry == "Tick", "Export must retain selected profiler");
Check(Widgets.ExportButton is Rect b && b.y >= window.Received.yMax, "DPA content must not overlap export toolbar");
Check(!Widgets.Click, "Handled export click must be consumed");
// Ordinary entry clicks must still reach DPA.
Widgets.MouseX = 50; Widgets.MouseY = 100; Widgets.Click = true;
window.DoWindowContents(new Rect(0, 0, 1000, 700));
Check(Window_Analyzer.Entry == "Update" && Exporter.Exports == 1, "DPA navigation must remain usable");
// Repaint and varying window geometry: toolbar stays inside outer window, outside DPA.
foreach (var outer in new[] { new Rect(0,0,210,158), new Rect(11,23,640,400), new Rect(0,0,1600,900) })
{
    Widgets.ExportButton = null; Widgets.Click = false;
    window.DoWindowContents(outer);
    Check(Widgets.ExportButton is Rect button && button.y >= window.Received.yMax &&
        button.yMax <= outer.yMax-18 && button.x >= outer.x+18 && button.x+button.width <= outer.x+outer.width-18,
        "Reserved toolbar must fit after resize and nonzero origin");
}
Widgets.ExportButton = null;
window.DoWindowContents(new Rect(0,0,100,100));
Check(Widgets.ExportButton == null && window.Received.height == 100, "Undersized windows must retain original geometry");
Check(Exporter.Exports == 1, "Repaints must not repeat exports");
Console.WriteLine("GUI regression passed: real Harmony binding, export routing, selection, layout, navigation and resize");
