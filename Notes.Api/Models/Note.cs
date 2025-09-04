// Note.cs
// Purpose: EF Core entity that maps to the "Notes" table in the database.
// Each instance represents one row.
//
// Location: Notes.Api/Models/Note.cs
// Namespace must match your project root namespace ("Notes.Api") plus ".Models".

namespace Notes.Api.Models
{
    public class Note
    {
        // Primary key. In SQLite this becomes INTEGER PRIMARY KEY AUTOINCREMENT.
        public int Id { get; set; }

        // Short, human-readable title of the note.
        public string Title { get; set; } = string.Empty;

        // Free-form note content (plain text or markdown).
        public string Content { get; set; } = string.Empty;

        // Timestamps stored in UTC.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
