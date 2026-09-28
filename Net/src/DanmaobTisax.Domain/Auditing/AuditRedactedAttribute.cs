namespace DanmaobTisax.Domain.Auditing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditRedactedAttribute : Attribute
{
}
