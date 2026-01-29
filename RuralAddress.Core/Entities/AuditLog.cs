using System;

namespace RuralAddress.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty; // e.g., "Propriedade"
    public string EntityId { get; set; } = string.Empty;   // Primary Key of the entity
    public string Action { get; set; } = string.Empty;     // "Create", "Update", "Delete"
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }                    // Who made the change
    public string? Changes { get; set; }                   // JSON or formatted string of changes
    
    // Support for Aggregate Root linking
    public string? ParentEntityName { get; set; }
    public string? ParentEntityId { get; set; }
}
