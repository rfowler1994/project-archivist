using Archivist.Api.Models;
namespace Archivist.Api.Dtos.QuickNotes;

public sealed class QuickNoteCreateRequest
{
    public string? Title { get; set; }

    public required string Body { get; set; }

    public QuickNoteState? State { get; set; }
}