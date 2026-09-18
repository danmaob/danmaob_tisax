using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DanmaobTisax.Infrastructure.Auditing;

public static class AuditValueSerializer
{
    private static string SerializeProperties(EntityEntry entry, Func<string, object?> getValue)
    {
        var properties = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            try
            {
                properties[property.Metadata.Name] = getValue(property.Metadata.Name);
            }
            catch
            {
                properties[property.Metadata.Name] = "<unserializable>";
            }
        }

        return JsonSerializer.Serialize(properties);
    }

    public static string SerializeAllCurrentValues(EntityEntry entry)
    {
        return SerializeProperties(entry, name => entry.Property(name).CurrentValue);
    }

    public static string SerializeAllOriginalValues(EntityEntry entry)
    {
        return SerializeProperties(entry, name => entry.Property(name).OriginalValue);
    }

    public static string SerializeModifiedOriginalValues(EntityEntry entry)
    {
        var modifiedProperties = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (entry.Property(property.Metadata.Name).IsModified)
            {
                try
                {
                    modifiedProperties[property.Metadata.Name] = entry.Property(property.Metadata.Name).OriginalValue;
                }
                catch
                {
                    modifiedProperties[property.Metadata.Name] = "<unserializable>";
                }
            }
        }

        return JsonSerializer.Serialize(modifiedProperties);
    }

    public static string SerializeModifiedCurrentValues(EntityEntry entry)
    {
        var modifiedProperties = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (entry.Property(property.Metadata.Name).IsModified)
            {
                try
                {
                    modifiedProperties[property.Metadata.Name] = entry.Property(property.Metadata.Name).CurrentValue;
                }
                catch
                {
                    modifiedProperties[property.Metadata.Name] = "<unserializable>";
                }
            }
        }

        return JsonSerializer.Serialize(modifiedProperties);
    }

    public static string SerializeModifiedPropertyNames(EntityEntry entry)
    {
        var modifiedPropertyNames = new List<string>();

        foreach (var property in entry.Properties)
        {
            if (entry.Property(property.Metadata.Name).IsModified)
            {
                modifiedPropertyNames.Add(property.Metadata.Name);
            }
        }

        return JsonSerializer.Serialize(modifiedPropertyNames);
    }
}
