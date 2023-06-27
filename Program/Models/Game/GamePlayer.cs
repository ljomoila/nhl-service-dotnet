namespace nhl_service_dotnet.Models.Game
{
    public class GamePlayer : Player
    {
        public int assists { get; set; }
        public int goals { get; set; }
        public string? points { get; set; }
    }
}
