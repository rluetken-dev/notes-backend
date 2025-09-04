// Contracts/NoteDtos.cs
// Purpose: API-facing Data Transfer Objects (DTOs) with validation attributes.
// These define what clients are allowed to send to the API, separate from the EF entity.

using System.ComponentModel.DataAnnotations;

namespace Notes.Api.Contracts
{
    public class CreateNoteRequest
    {
        // Required title with reasonable length limits for UX and DB safety.
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        // Optional content; limit to a few KB for demo purposes.
        [StringLength(4000)]
        public string? Content { get; set; } = string.Empty;
    }

    public class UpdateNoteRequest
    {
        // For PUT we also require a non-empty title.
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Content { get; set; } = string.Empty;
    }
}
