using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
