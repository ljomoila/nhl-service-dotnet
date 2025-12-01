namespace nhl_service_dotnet.Models.Game
{
    public class Game
    {
        public GameTeam? home { get; set; }
        public GameTeam? away { get; set; }
        public string? timeRemaining { get; set; }
        public string? period { get; set; }
        public string? status { get; set; }

    }

    public enum GameStatus
    {
        InProgress,
        Final,
        Scheduled
    }
}
