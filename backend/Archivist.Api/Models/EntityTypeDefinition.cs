namespace Archivist.Api.Models;

public class EntityTypeDefinition
{
    // Primary Key
    public Guid Id { get; set; }

    // Name of Entity Type
    public required string Name { get; set; }

    // A short description explaining the Entity Type
    public string? Description { get; set; }
}