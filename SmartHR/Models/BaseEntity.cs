namespace SmartHR.Models
{
    /// <summary>
    /// Base entity class providing common fields for all entities
    /// including audit tracking and soft delete support
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Primary key identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// UTC timestamp when the record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC timestamp when the record was last updated
        /// Enables audit trails and data modification tracking
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft delete flag - when true, record is logically deleted
        /// but retained in database for compliance and audit purposes
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// UTC timestamp when the record was soft-deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
