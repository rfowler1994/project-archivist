namespace Archivist.Api.Models;

// Defines what fields each entity type has
public class FieldDefinition
{
    // Primary Key
    public Guid Id { get; set; }

    // Entity Type
    public required String EntityTypeName { get; set; }

    public required String FieldName { get; set; }

    public FieldType FieldType { get; set; } = FieldType.Text;

    // Type-specific configuration
    public Dictionary<string, object>? Configuration { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
}

public enum FieldType
{
    Text,
    LongText,
    Number,
    Boolean,
    Date,
    SingleSelect,
    MultiSelect,
    EntityReference,
    EntityReferenceList
}