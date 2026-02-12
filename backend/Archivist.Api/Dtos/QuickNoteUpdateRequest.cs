using Archivist.Api.Models;
namespace Archivist.Api.Dtos.QuickNotes;

public sealed class QuickNoteUpdateRequest
{
    public string? Title { get; set; }

    public required string Body { get; set; }

    public QuickNoteState? State { get; set; }
}