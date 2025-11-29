namespace nhl_service_dotnet.Models.Game
{
    public class GamePlayer : Player
    {
        public int assists { get; set; }
        public int goals { get; set; }
        public string? points { get; set; }
        public string? position { get; set; }
    }

    public class GameGoalie : GamePlayer
    {
        public int saves { get; set; }
        public string? saveShotsAgainst { get; set; }
        public double savePercentage { get; set; }
    }
}
