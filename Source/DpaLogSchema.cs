using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DpaCsvExporter;

/// <summary>Validate DPA's public row contract once per runtime type, before output.</summary>
internal sealed class DpaLogSchema
{
    private static readonly ConcurrentDictionary<Type, DpaLogSchema> Cache = new();
    private readonly Dictionary<string, FieldInfo?> fields = new(StringComparer.Ordinal);

    public static DpaLogSchema For(Type type) => Cache.GetOrAdd(type, t => new DpaLogSchema(t));

    private DpaLogSchema(Type type)
    {
        Add(type, "entries", typeof(int), required: true);
        Add(type, "average", typeof(double), required: true);
        Add(type, "self", typeof(double), required: true);
        Add(type, "max", typeof(float), required: true);
        Add(type, "total", typeof(float), required: true);
        Add(type, "calls", typeof(float), required: true);
        Add(type, "percent", typeof(float), required: true);
        Add(type, "key", typeof(string));
        Add(type, "label", typeof(string));
        Add(type, "type", typeof(Type));
        Add(type, "meth", typeof(MethodBase));
        Add(type, "spikes", typeof(int));
        Add(type, "spike", typeof(int));
        Add(type, "seenIn", typeof(int));
        Add(type, "pinned", typeof(bool));
    }

    private void Add(Type type, string name, Type expected, bool required = false)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if ((field == null && required) || (field != null && !expected.IsAssignableFrom(field.FieldType)))
            throw new InvalidOperationException(
                $"DPA compatibility error: {type.FullName}.{name} must be {expected.FullName}; " +
                $"found {field?.FieldType.FullName ?? "missing field"}. No CSV was saved.");
        fields.Add(name, field);
    }

    public object? Read(object row, string name) => fields[name]?.GetValue(row);
}
