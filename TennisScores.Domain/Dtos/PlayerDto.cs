namespace TennisScores.Domain.Dtos
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? CurrentScore { get; set; }
    }
}