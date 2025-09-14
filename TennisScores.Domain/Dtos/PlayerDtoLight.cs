namespace TennisScores.Domain.Dtos
{
    public class PlayerLightDto
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int? Age { get; set; }
    }
}