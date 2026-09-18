namespace DanmaobTisax.Application.Auditing;

public sealed class AuditLogDto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set;  }

    public string? EntityName { get; set;  }

    public string? EntityId { get; set;  }

    public string? Action { get; set;  }

    public Guid? PerformedByUserId { get;  set; }

    public string? PerformedByDisplayName { get; set;  }

    public DateTime PerformedAtUtc { get; set;  }

    public string? OldValuesJson { get;  set; }

    public string? NewValuesJson { get;  set; }

    public string? ChangedColumnsJson { get;  set; }

}