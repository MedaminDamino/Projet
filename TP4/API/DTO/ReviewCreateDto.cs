using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class ReviewCreateDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? ReviewText { get; set; }
    }

    public class ReviewUpdateDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? ReviewText { get; set; }
    }
}
