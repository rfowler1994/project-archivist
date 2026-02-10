namespace Archivist.Api.Models;

// Represents the actual entities
public class TypedEntity
{
    // Primary Key
    public Guid Id { get; set; }

    // Entity Type
    public required String EntityTypeName { get; set; }

     // A short, display-friendly name or title.
    public required string Title { get; set; }

    // When the Entity was created (server-assigned).
    public DateTime CreatedAt { get; set; }

    // When the Entity was last updated (server-assigned).
    public DateTime UpdatedAt { get; set; }

    // Extraction Tracking. Where the entity was extracted from
    public Guid? SourceEntityId { get; set; }
    public string? SourceEntityType { get; set; }

    // Markdown content of the Entity
    public string Body { get; set; } = string.Empty;

    public Dictionary<string, object>? CustomFieldsData { get; set; }

    

    
}