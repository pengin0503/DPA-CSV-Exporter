// Test-only UI boundary: real addon + real Harmony, no Unity player or game DLLs.
using System.Runtime.CompilerServices;
namespace UnityEngine
{
    public struct Rect(float x, float y, float width, float height)
    {
        public float x = x, y = y, width = width, height = height;
        public float yMax => y + height;
        public bool Contains(float px, float py) => px >= x && px < x + width && py >= y && py < yMax;
    }
}
namespace RimWorld
{
    public static class MessageTypeDefOf { public static readonly object TaskCompletion = new(), RejectInput = new(); }
}
namespace Verse
{
    using UnityEngine;
    public class ModContentPack { }
    public class Mod
    {
        public Mod(ModContentPack content) { _ = content; }
        public virtual string SettingsCategory() => "";
        public virtual void DoSettingsWindowContents(Rect rect) { }
    }
    public static class LongEventHandler { public static void ExecuteWhenFinished(Action action) => action(); }
    public static class Log
    {
        public static void Message(string text) { }
        public static void Warning(string text) => throw new Exception(text);
        public static void Error(string text) => throw new Exception(text);
        public static void ErrorOnce(string text, int key) => throw new Exception(text);
    }
    public static class Messages { public static void Message(string text, object type, bool historical) { } }
    public static class RectExtensions
    {
        public static Rect ContractedBy(this Rect r, float n) => new(r.x+n, r.y+n, r.width-2*n, r.height-2*n);
    }
    public static class Widgets
    {
        public static float MouseX, MouseY;
        public static bool Click;
        public static Rect? ExportButton;
        public static void Label(Rect rect, string text) { }
        public static bool ButtonText(Rect rect, string text)
        {
            if (text == "Export CSV") ExportButton = rect;
            return ButtonInvisible(rect);
        }
        public static bool ButtonInvisible(Rect rect)
        {
            if (!Click || !rect.Contains(MouseX, MouseY)) return false;
            Click = false;
            return true;
        }
    }
}
namespace DpaCsvExporter
{
    public static class Exporter
    {
        public static int Exports;
        public static string ExportedEntry = "";
        public static string ExportCurrent()
        {
            Exports++;
            ExportedEntry = Analyzer.Window_Analyzer.Entry;
            return "fixture.csv";
        }
    }
}
namespace Analyzer
{
    using UnityEngine;
    using Verse;
    public class Window_Analyzer
    {
        public static string Entry = "Tick";
        public Rect Received;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DoWindowContents(Rect inRect)
        {
            Received = inRect;
            // DPA Panel_Tabs draws invisible entry buttons before the old postfix.
            if (Widgets.ButtonInvisible(new Rect(inRect.x+18, inRect.y+18, 202, inRect.height-36)))
                Entry = "Update";
        }
    }
}
