using Archivist.Api.Data;
using Archivist.Api.Dtos.QuickNotes;
using Archivist.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Archivist.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuickNotesController : ControllerBase
    {
        private readonly ArchivistDbContext _db;

        // DI injects the DbContext configured in Program.cs
        public QuickNotesController(ArchivistDbContext db)
        {
            _db = db;
        }

        // POST /api/quicknotes
        [HttpPost]
        public async Task<ActionResult<QuickNoteResponse>> Create([FromBody] QuickNoteCreateRequest request)
        {
            // Validation: prevent empty notes.
            if (string.IsNullOrWhiteSpace(request.Body)) return BadRequest("Note Body is required.");

            var now = DateTime.UtcNow;
            var note = new QuickNote

            {
                Id = Guid.NewGuid(),

                // From Request
                Title = request.Title,
                Body = request.Body,
                State = request.State ?? QuickNoteState.Open,

                // UTC to avoid timezone bugs.
                CreatedAt = now,
                UpdatedAt = now,
                DeletedAt = null
            };
            // Execute SQL (INSERT) against the database.
            _db.QuickNotes.Add(note);
            await _db.SaveChangesAsync();

            var response = ToResponse(note);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        // GET /api/quicknotes/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuickNoteResponse>> GetById(Guid id)
        {
            var note = await _db.QuickNotes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            // If it doesn't exist, return 404.
            if (note is null) return NotFound();
            else return Ok(ToResponse(note));
        }

        // GET /api/quicknotes?page=1&pageSize=100
        [HttpGet]
        public async Task<ActionResult<List<QuickNoteInboxItem>>> List(
            [FromQuery] string view = "inbox",
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] int previewLength = 100)
        {

            // Safety: page must be 1+
            if (page < 1) page = 1;
            // Safety: cap pageSize
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.QuickNotes.AsNoTracking();

            query = view.ToLowerInvariant() switch
            {
                "trash" => query.Where(n=> n.DeletedAt != null),
                _ => query.Where(n=> n.DeletedAt == null)       // Default View is Inbox
            };
            
            var notesList = await query
                .OrderByDescending(n => n.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new QuickNoteInboxItem
                {
                    Id = n.Id,
                    Title = n.Title,
                    UpdatedAt = n.UpdatedAt,
                    // Safety: Avoid substring past the end.
                    Preview = n.Body.Length <= previewLength ? n.Body
                        : n.Body.Substring(0, previewLength)
                })
                .ToListAsync();

            return Ok(notesList);
        }

        // PUT /api/quicknotes/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<QuickNoteResponse>> Update(Guid id, [FromBody] QuickNoteUpdateRequest request)
        {
            // Only allow updating non-deleted notes
            var note = await _db.QuickNotes.FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);
            if (note is null) return NotFound();

            note.Title = request.Title;
            note.Body = request.Body;
            note.State = request.State ?? QuickNoteState.Open;
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(ToResponse(note));
        }

        // DELETE /api/quicknotes/{id}
        // Soft delete: set DeletedAt.
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var note = await _db.QuickNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (note is null) return NotFound();

            // Already Deleted. Nothing to do.
            if (note.DeletedAt is not null) return NoContent();

            var now = DateTime.UtcNow;
            note.DeletedAt = now;
            note.UpdatedAt = now;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/quicknotes/{id}/restore
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var note = await _db.QuickNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (note is null) return NotFound();

            // Not Deleted. Nothing to do.
            if (note.DeletedAt is null) return NoContent();

            note.DeletedAt = null;
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/quicknotes/{id}/hard
        // Permanent delete: removes the row.
        [HttpDelete("{id:guid}/hard")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var note = await _db.QuickNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (note is null) return NotFound();

            _db.QuickNotes.Remove(note);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static QuickNoteResponse ToResponse(QuickNote note) => new()
        {
            Id = note.Id,
            Title = note.Title,
            Body = note.Body,
            State = note.State,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            DeletedAt = note.DeletedAt
        };
    }

    public class QuickNoteInboxItem
    {
        public Guid Id { get; set; }               // Which note to open
        public string? Title { get; set; }
        public DateTime UpdatedAt { get; set; }    // For sorting + “last updated” label
        public string Preview { get; set; } = string.Empty;  // First N chars of body
    }
}