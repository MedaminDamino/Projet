using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.DTO
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(3)]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}
