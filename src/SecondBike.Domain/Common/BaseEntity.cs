namespace SecondBike.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides common audit properties.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created. Set automatically by the DbContext.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated. Set automatically by the DbContext.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. When true, the entity is considered deleted but remains in the database.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The user or system identifier who created this entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// The user or system identifier who last modified this entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
