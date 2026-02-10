namespace Archivist.Api.Models;

// This class represents a Quick Note entity 
public class QuickNote
{
    // Primary Key
    public Guid Id { get; set; }

    // Markdown content of the note
    public string Body { get; set; } = string.Empty;

    // Datetime when this note was created
    public DateTime CreatedAt { get; set; }

    // Datetime when this note was last modified
    public DateTime UpdatedAt { get; set; }

}