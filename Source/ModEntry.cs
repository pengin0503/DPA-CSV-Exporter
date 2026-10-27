using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DpaCsvExporter;

/// <summary>Inject a single export button into the Dubs Performance Analyzer window.</summary>
public sealed class DpaCsvExporterMod : Mod
{
    private static string lastStatus = "No export attempted yet. Select a profiler entry, then export all rows.";

    public DpaCsvExporterMod(ModContentPack content) : base(content)
    {
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            try
            {
                var target = AccessTools.Method("Analyzer.Window_Analyzer:DoWindowContents");
                if (target == null)
                {
                    Log.Warning("[DPA CSV Exporter] Dubs Performance Analyzer window was not found.");
                    return;
                }
                new Harmony("pengin0503.dpa.csvexporter").Patch(target,
                    prefix: new HarmonyMethod(typeof(DpaCsvExporterMod), nameof(DrawExportButton)));
                Log.Message("[DPA CSV Exporter] Export CSV button attached.");
            }
            catch (Exception e)
            {
                Log.Error("[DPA CSV Exporter] Could not add export button: " + e);
            }
        });
    }

    public override string SettingsCategory() => "DPA CSV Exporter";

    public override void DoSettingsWindowContents(Rect rect)
    {
        var inner = rect.ContractedBy(12f);
        Widgets.Label(new Rect(inner.x, inner.y, inner.width, 65f),
            "Open Dubs Performance Analyzer and select a Tick, Update, GUI, or other profiler entry.\n" +
            "Export all rows in the active profiler, including rows hidden by its search filter.\n" +
            "CSV folder: RimWorld user data / AnalyzerExports");
        if (Widgets.ButtonText(new Rect(inner.x, inner.y + 75f, 250f, 34f), "Export all current measurements"))
            ExportWithFeedback();
        Widgets.Label(new Rect(inner.x, inner.y + 125f, inner.width, Math.Max(90f, inner.height - 130f)), lastStatus);
    }

    // Reserve a footer before DPA lays out its tabs, logs, graph and drag handles.
    // A postfix overlay is too late: an underlying invisible tab button can
    // consume the click and switch/clear the active profiler before we see it.
    public static void DrawExportButton(ref Rect inRect)
    {
        try
        {
            const float footerHeight = 48f;
            if (inRect.width < 210f || inRect.height < 110f + footerHeight) return;
            var button = new Rect(inRect.x + 24f, inRect.yMax - 44f, 168f, 26f);
            inRect.height -= footerHeight;
            // Process input first; the original window receives only the remaining
            // content rect, so its controls never occupy the export button's area.
            if (Widgets.ButtonText(button, "Export CSV"))
                ExportWithFeedback();
        }
        catch (Exception e)
        {
            Log.ErrorOnce("[DPA CSV Exporter] Export GUI error: " + e, 172890412);
        }
    }

    private static void ExportWithFeedback()
    {
        try
        {
            var path = Exporter.ExportCurrent();
            lastStatus = "Saved: " + path;
            Log.Message("[DPA CSV Exporter] Saved: " + path);
            Messages.Message("DPA CSV saved: " + System.IO.Path.GetFileName(path),
                MessageTypeDefOf.TaskCompletion, false);
        }
        catch (Exception e)
        {
            lastStatus = "Export failed: " + e.Message;
            Log.Warning("[DPA CSV Exporter] Export failed: " + e);
            Messages.Message("DPA CSV export failed: " + e.Message,
                MessageTypeDefOf.RejectInput, false);
        }
    }
}
