using Archivist.Api.Data;
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

        // Request shape for creating a note.
        public class CreateQuickNoteRequest
        {
            public required string Body { get; set; }
        }

        // POST /api/quicknotes
        [HttpPost]
        public async Task<ActionResult<QuickNote>> Create([FromBody] CreateQuickNoteRequest request)
        {
            // Validation: prevent empty notes.
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                // 400 Bad Request with a simple error message.
                return BadRequest("Body is required.");
            }

            // Create a new entity instance.
            var note = new QuickNote
            {
                // New GUID for the primary key.
                Id = Guid.NewGuid(),

                // Store the markdown body.
                Body = request.Body,

                // Use UTC to avoid timezone bugs later.
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Stage the insert.
            _db.QuickNotes.Add(note);

            // Execute SQL (INSERT) against the database.
            await _db.SaveChangesAsync();

            // Return 201 Created + location header for the new resource.
            // Points to GET /api/quicknotes/{id}
            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }

        // GET /api/quicknotes/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuickNote>> GetById(Guid id)
        {
            var note = await _db.QuickNotes.FindAsync(id);

            // If it doesn't exist, return 404.
            if (note is null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        // GET /api/quicknotes?page=1&pageSize=100
        [HttpGet]
        public async Task<ActionResult<List<QuickNote>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            const int previewLength = 100;

            // Safety: page must be 1+
            if (page < 1) page = 1;
            // Safety: cap pageSize
            pageSize = Math.Clamp(pageSize, 1, 100);
            
            var notes = await _db.QuickNotes
                .AsNoTracking()
                .OrderByDescending(n => n.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new QuickNoteInboxItem
                {
                    Id = n.Id,
                    UpdatedAt = n.UpdatedAt,
                    // Safety: Avoid substring past the end.
                    Preview = n.Body.Length <= previewLength ? n.Body
                        : n.Body.Substring(0, previewLength)
                })
                .ToListAsync();

            return Ok(notes);
        }
    }

    public class QuickNoteInboxItem
    {
        public Guid Id { get; set; }               // Which note to open
        public DateTime UpdatedAt { get; set; }    // For sorting + “last updated” label
        public string Preview { get; set; } = "";  // First N chars of body (or title later)
    }
}