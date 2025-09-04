// Controllers/NotesController.cs
// Purpose: Expose HTTP endpoints (Web API) to create notes.
// This first version only implements POST /api/notes.
// Later we will add GET/PUT/DELETE step by step.

using Microsoft.AspNetCore.Mvc;
using Notes.Api.Data;
using Notes.Api.Models;
using Microsoft.EntityFrameworkCore;    // for ToListAsync() and ordering
using Microsoft.AspNetCore.Http;        // for StatusCodes in Swagger annotations
using Notes.Api.Contracts;              // our DTOs

namespace Notes.Api.Controllers
{
    // Marks this class as a Web API controller (automatic model binding, validation, etc.)
    [ApiController]

    // Base route for this controller. With class name "NotesController" this becomes "api/notes".
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly AppDb _db;

        // DbContext is provided by ASP.NET Core dependency injection
        public NotesController(AppDb db) => _db = db;

        // 201 Created with a Location header that points to GET /api/notes/{id}
        // [ApiController] automatically validates the DTO based on data annotations.
        [ProducesResponseType(typeof(Note), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost] // => POST /api/notes
        public async Task<ActionResult<Note>> Create([FromBody] CreateNoteRequest input)
        {
            // No manual ModelState checks needed; [ApiController] returns 400 if invalid.
            var entity = new Note
            {
                Title = input.Title.Trim(),
                Content = (input.Content ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _db.Notes.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = entity.Id },
                value: entity
            );
        }

        // Returns a single note by its primary key.
        // - Route constraint "{id:int}" ensures only integers match.
        // - EF Core FindAsync(pk) is fastest for primary key lookups.
        [HttpGet("{id:int}")] // => GET /api/notes/123
        public async Task<ActionResult<Note>> GetById(int id)
        {
            var note = await _db.Notes.FindAsync(id);
            if (note is null)
                return NotFound(); // 404 if the note does not exist

            return Ok(note);       // 200 with the note as JSON
        }

        // Returns notes with optional text filter, pagination, and sorting.
        // Query params:
        // - q: optional search term (case-insensitive contains on title/content)
        // - page: 1-based page index (default 1)
        // - pageSize: items per page (default 10; max 100)
        // - sort: one of ["id","title","created","updated"] (default "updated")
        // - dir:  one of ["asc","desc"] (default "desc")
        [HttpGet] // => GET /api/notes?q=foo&page=1&pageSize=10&sort=title&dir=asc
        public async Task<ActionResult<List<Note>>> GetAll(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sort = "updated",
            [FromQuery] string? dir = "desc")
        {
            // --- Validate and normalize paging ---
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            // --- Validate sort/dir against a whitelist ---
            var allowedSort = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "id", "title", "created", "createdat", "updated", "updatedat" };
            var allowedDir = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "asc", "desc" };

            var by = (sort ?? "updated").Trim();
            var ord = (dir ?? "desc").Trim();

            if (!allowedSort.Contains(by))
                return BadRequest("Invalid 'sort'. Allowed: id, title, created, updated.");
            if (!allowedDir.Contains(ord))
                return BadRequest("Invalid 'dir'. Allowed: asc, desc.");

            var desc = ord.Equals("desc", StringComparison.OrdinalIgnoreCase);

            // --- Build base query (deferred) ---
            var query = _db.Notes.AsQueryable();

            // Optional filter: case-insensitive contains on title/content
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(term) ||
                    n.Content.ToLower().Contains(term));
            }

            // Total BEFORE paging (for client-side pagination UI)
            var total = await query.CountAsync();

            // --- Sorting: choose primary key and add a stable secondary key on Id ---
            IOrderedQueryable<Note> ordered = by.ToLowerInvariant() switch
            {
                "title" => desc ? query.OrderByDescending(n => n.Title)
                                : query.OrderBy(n => n.Title),

                "created" or "createdat" => desc ? query.OrderByDescending(n => n.CreatedAt)
                                                 : query.OrderBy(n => n.CreatedAt),

                "updated" or "updatedat" => desc ? query.OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                                                 : query.OrderBy(n => n.UpdatedAt ?? n.CreatedAt),

                "id" => desc ? query.OrderByDescending(n => n.Id)
                             : query.OrderBy(n => n.Id),

                _ => desc ? query.OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                          : query.OrderBy(n => n.UpdatedAt ?? n.CreatedAt)
            };

            ordered = desc ? ordered.ThenByDescending(n => n.Id) : ordered.ThenBy(n => n.Id);

            // --- Apply paging and execute ---
            var items = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Expose total count for pagination UI
            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(items);
        }

        // 200 OK with the updated resource or 404 if the id is unknown.
        [ProducesResponseType(typeof(Note), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}")] // => PUT /api/notes/123
        public async Task<ActionResult<Note>> Update(int id, [FromBody] UpdateNoteRequest input)
        {
            var note = await _db.Notes.FindAsync(id);
            if (note is null)
                return NotFound();

            note.Title = input.Title.Trim();
            note.Content = (input.Content ?? string.Empty).Trim();
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(note);
        }

        // Deletes a note by id.
        // Semantics: DELETE is idempotent (calling it twice for the same id should be safe).
        [HttpDelete("{id:int}")] // => DELETE /api/notes/123
        public async Task<IActionResult> Delete(int id)
        {
            // Try load the entity by primary key
            var note = await _db.Notes.FindAsync(id);
            if (note is null)
                return NotFound(); // 404 if it does not exist

            // Remove and persist
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();

            // 204 No Content indicates success with no response body
            return NoContent();
        }
    }
}
