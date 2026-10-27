using Analyzer;
using DpaCsvExporter;
using UnityEngine;
using Verse;
static void Check(bool ok, string reason) { if (!ok) throw new Exception(reason); }
var mod = new DpaCsvExporterMod(new ModContentPack());
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

// The OS/clipboard are boundary fixtures; routing is the real ModEntry code.
Widgets.Buttons.Clear(); Widgets.Click = false;
mod.DoSettingsWindowContents(new Rect(0,0,640,500));
Check(Widgets.Buttons.ContainsKey("Open folder") && Widgets.Buttons.ContainsKey("Copy path"), "Settings must expose both output-folder actions");
var open = Widgets.Buttons["Open folder"];
Widgets.MouseX = open.x+5; Widgets.MouseY = open.y+5; Widgets.Click = true;
mod.DoSettingsWindowContents(new Rect(0,0,640,500));
Check(OutputFolder.OpenCalls == 1 && Exporter.Exports == 1, "Open folder must route to OS boundary without re-export");
var copy = Widgets.Buttons["Copy path"];
Widgets.MouseX = copy.x+5; Widgets.MouseY = copy.y+5; Widgets.Click = true;
mod.DoSettingsWindowContents(new Rect(0,0,640,500));
Check(GUIUtility.systemCopyBuffer == OutputFolder.Path && OutputFolder.OpenCalls == 1, "Copy must preserve the literal path without opening a process");
Widgets.Buttons.Clear(); Widgets.Click = false;
window.DoWindowContents(new Rect(0,0,1000,700));
Check(Widgets.Buttons.ContainsKey("Open folder") && Widgets.Buttons.ContainsKey("Copy path"), "Wide DPA footer must expose folder actions");
Check(Widgets.Buttons["Open folder"].y >= window.Received.yMax && Widgets.Buttons["Copy path"].y >= window.Received.yMax,
    "Folder controls must stay outside DPA content");
Widgets.Buttons.Clear();
window.DoWindowContents(new Rect(0,0,210,158));
Check(Widgets.Buttons.ContainsKey("Export CSV") && !Widgets.Buttons.ContainsKey("Open folder"), "Narrow DPA must retain export without overflowing folder controls");
Widgets.Buttons.Clear();
mod.DoSettingsWindowContents(new Rect(0,0,210,400));
Check(Widgets.Buttons.ContainsKey("Open folder") && Widgets.Buttons["Copy path"].x+Widgets.Buttons["Copy path"].width <= 198,
    "Folder actions must remain available in narrow settings");
Console.WriteLine("Folder action GUI routing, clipboard content and responsive layout passed");

OutputFolder.FailOpen = true;
open = Widgets.Buttons["Open folder"];
Widgets.MouseX = open.x+5; Widgets.MouseY = open.y+5; Widgets.Click = true;
mod.DoSettingsWindowContents(new Rect(0,0,210,400));
Check(Messages.LastText.Contains("Folder action failed") && Log.LastWarning.Contains("simulated OS failure"),
    "An unavailable OS folder opener must report failure without breaking settings");
Console.WriteLine("Folder action error feedback passed");
