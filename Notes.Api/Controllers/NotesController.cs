using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Api.Contracts;
using Notes.Api.Data;
using Notes.Api.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Notes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "title",
            "created",
            "createdat",
            "updated",
            "updatedat"
        };

        private static readonly HashSet<string> AllowedSortDirections = new(StringComparer.OrdinalIgnoreCase)
        {
            "asc",
            "desc"
        };

        private readonly AppDb _db;

        public NotesController(AppDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/notes
        ///     {
        ///        "title": "Shopping",
        ///        "content": "Milk, Bread, Eggs"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created note</response>
        /// <response code="400">If the input is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(Note), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Note>> Create([FromBody] CreateNoteRequest input)
        {
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
                nameof(GetById),
                new { id = entity.Id },
                entity);
        }

        /// <summary>
        /// Returns a single note by its ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/notes/1
        ///
        /// </remarks>
        /// <response code="200">Returns the note with the specified ID</response>
        /// <response code="404">If the note does not exist</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Note), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Note>> GetById(int id)
        {
            var note = await _db.Notes.FindAsync(id);

            if (note is null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        /// <summary>
        /// Returns a paginated list of notes, optionally filtered and sorted.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/notes?q=shopping&amp;page=1&amp;pageSize=5&amp;sort=title&amp;dir=asc
        ///
        /// Query parameters:
        /// - q: optional search term (case-insensitive, matches title or content)
        /// - page: 1-based page index (default = 1)
        /// - pageSize: number of items per page (default = 10, max = 100)
        /// - sort: one of [id, title, created, updated] (default = updated)
        /// - dir: sort direction, either asc or desc (default = desc)
        ///
        /// Response header:
        /// - X-Total-Count: total number of matching notes (before paging)
        /// </remarks>
        /// <response code="200">Returns the list of notes</response>
        /// <response code="400">If one of the query parameters is invalid</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Returns a paginated list of notes, optionally filtered and sorted.",
            Description = "Supports q, page, pageSize, sort, dir. Response header X-Total-Count contains the total items.")]
        [ProducesResponseType(typeof(List<Note>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<Note>>> GetAll(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sort = "updated",
            [FromQuery] string? dir = "desc")
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var sortBy = (sort ?? "updated").Trim();
            var sortDirection = (dir ?? "desc").Trim();

            if (!AllowedSortFields.Contains(sortBy))
            {
                return BadRequest("Invalid 'sort'. Allowed: id, title, created, updated.");
            }

            if (!AllowedSortDirections.Contains(sortDirection))
            {
                return BadRequest("Invalid 'dir'. Allowed: asc, desc.");
            }

            var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

            var query = _db.Notes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();

                query = query.Where(note =>
                    note.Title.ToLower().Contains(term) ||
                    note.Content.ToLower().Contains(term));
            }

            var total = await query.CountAsync();
            var ordered = ApplySorting(query, sortBy, descending);

            var items = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(items);
        }

        /// <summary>
        /// Updates an existing note by its ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/notes/1
        ///     {
        ///        "title": "Updated shopping list",
        ///        "content": "Milk, Bread, Eggs, Butter"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns the updated note</response>
        /// <response code="400">If the input is invalid</response>
        /// <response code="404">If the note does not exist</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Note), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Note>> Update(int id, [FromBody] UpdateNoteRequest input)
        {
            var note = await _db.Notes.FindAsync(id);

            if (note is null)
            {
                return NotFound();
            }

            note.Title = input.Title.Trim();
            note.Content = (input.Content ?? string.Empty).Trim();
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(note);
        }

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/notes/1
        ///
        /// </remarks>
        /// <response code="204">The note was successfully deleted</response>
        /// <response code="404">If the note does not exist</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _db.Notes.FindAsync(id);

            if (note is null)
            {
                return NotFound();
            }

            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static IOrderedQueryable<Note> ApplySorting(
            IQueryable<Note> query,
            string sortBy,
            bool descending)
        {
            IOrderedQueryable<Note> ordered = sortBy.ToLowerInvariant() switch
            {
                "title" => descending
                    ? query.OrderByDescending(note => note.Title)
                    : query.OrderBy(note => note.Title),

                "created" or "createdat" => descending
                    ? query.OrderByDescending(note => note.CreatedAt)
                    : query.OrderBy(note => note.CreatedAt),

                "updated" or "updatedat" => descending
                    ? query.OrderByDescending(note => note.UpdatedAt ?? note.CreatedAt)
                    : query.OrderBy(note => note.UpdatedAt ?? note.CreatedAt),

                "id" => descending
                    ? query.OrderByDescending(note => note.Id)
                    : query.OrderBy(note => note.Id),

                _ => descending
                    ? query.OrderByDescending(note => note.UpdatedAt ?? note.CreatedAt)
                    : query.OrderBy(note => note.UpdatedAt ?? note.CreatedAt)
            };

            return descending
                ? ordered.ThenByDescending(note => note.Id)
                : ordered.ThenBy(note => note.Id);
        }
    }
}