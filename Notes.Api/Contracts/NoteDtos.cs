using System.ComponentModel.DataAnnotations;

namespace Notes.Api.Contracts
{
    public class CreateNoteRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Content { get; set; } = string.Empty;
    }

    public class UpdateNoteRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Content { get; set; } = string.Empty;
    }
}