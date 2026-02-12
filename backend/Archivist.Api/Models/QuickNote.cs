namespace Archivist.Api.Models;

public enum QuickNoteState
{
    Open = 0,
    Closed = 1,
    Pinned = 2,
    Archived = 3
}

public class QuickNote
{
    // Primary Key
    public Guid Id { get; set; }

    // Optional Title
    public string? Title { get; set; }

    // Markdown content of the note
    public required string Body { get; set; }

    public QuickNoteState State { get; set; } = QuickNoteState.Open;

   // Server-assigned timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Soft delete
    public DateTime? DeletedAt { get; set; }

}