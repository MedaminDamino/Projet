namespace API.Dtos.Author
{
    public class AuthorReadDto
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }

    }
}
