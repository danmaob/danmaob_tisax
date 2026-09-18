namespace DanmaobTisax.Domain.Auditing;

/// <summary>
/// Marker interface to indicate that an entity supports auditing.
/// 
/// This interface enables opt-in auditability for entities that require tracking of changes.
/// Entities implementing this interface can be configured by the auditing infrastructure
/// to automatically log creation, updates, and deletions.
/// 
/// Auditability is NOT inherited from <see cref="BaseEntity"/>; it must be explicitly implemented.
/// </summary>
public interface IAuditable
{
}