namespace API.DTO
{
    public class ReadingGoalCreateDto
    {
        public int Year { get; set; }

        public int Goal { get; set; }

        public int Progress { get; set; }

        public int BookId { get; set; }
    }

    public class ReadingGoalUpdateDto : ReadingGoalCreateDto
    {
    }
}
