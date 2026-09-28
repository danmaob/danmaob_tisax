namespace DanmaobTisax.Infrastructure.Auditing
{
    /// <summary>
    /// Exception thrown when an attempt is made to modify or delete audit log records.
    /// </summary>
    public class AuditLogImmutableException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogImmutableException"/> class.
        /// </summary>
        public AuditLogImmutableException()
            : base("Audit log records are immutable and cannot be modified or deleted.")
        {
        }
    }
}