using System.Globalization;
using System.Text;
using DanmaobTisax.Application.Auditing;

namespace DanmaobTisax.Api.Auditing;

public static class PlatformAuditCsv
{
    public static string Build(IReadOnlyList<PlatformAuditLogDto> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("PerformedAtUtc,Action,EntityName,EntityId,AffectedTenantId,PerformedByUserId,PerformedByDisplayName,ChangedColumnsJson,OldValuesJson,NewValuesJson");
        foreach (var row in rows)
        {
            var fields = new[]
            {
                row.PerformedAtUtc.ToString("o", CultureInfo.InvariantCulture),
                row.Action,
                row.EntityName,
                row.EntityId,
                row.AffectedTenantId?.ToString() ?? string.Empty,
                row.PerformedByUserId?.ToString() ?? string.Empty,
                row.PerformedByDisplayName ?? string.Empty,
                row.ChangedColumnsJson ?? string.Empty,
                row.OldValuesJson ?? string.Empty,
                row.NewValuesJson ?? string.Empty
            };
            builder.AppendLine(string.Join(",", fields.Select(Escape)));
        }
        return builder.ToString();
    }

    private static string Escape(string value)
    {
        var safe = value;
        if (safe.Length > 0 && "=+-@".Contains(safe[0]) == true)
        {
            safe = "'" + safe;
        }
        return "\"" + safe.Replace("\"", "\"\"") + "\"";
    }
}
