namespace nhl_service_dotnet.Models.Game
{
    public class GameTeam : Team
    {
        public int goals { get; set; }
        public List<GamePlayer>? players { get; set; }
    }
}
