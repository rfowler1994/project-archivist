namespace Archivist.Api.Models;

public class QuickNote
{
    // Primary Key
    public Guid Id { get; set; }

    // Markdown content of the note
    public required string Body { get; set; }

   // When the Note was created (server-assigned).
    public DateTime CreatedAt { get; set; }

    // When the Note was last updated (server-assigned).
    public DateTime UpdatedAt { get; set; }

}