namespace TennisScores.Domain.Dtos
{
    public static class PlayerExtensions
    {
        public static PlayerLightDto ToLightDto(this Entities.Player player) => new()
        {
            Id = player.Id,
            FirstName = player.FirstName,
            LastName = player.LastName,
            Age = player.BirthDate == DateTime.MinValue ? null : (int?)(DateTime.Now.Year - player.BirthDate?.Year),
        };
    }
}