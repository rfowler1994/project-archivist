using Archivist.Api.Models;
namespace Archivist.Api.Dtos.QuickNotes;

public sealed class QuickNoteResponse
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string Body { get; set; } = string.Empty;

    public QuickNoteState State { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}