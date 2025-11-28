namespace nhl_service_dotnet.Models
{
    public class Player
    {
        public int id { get; set; }
        public string? fullName { get; set; }
        public string? lastName { get; set; }
        public string? nationality { get; set; }
        public string? link { get; set; }
        public PlayerType playerType { get; set; } = PlayerType.Skater;
    }

    public enum PlayerType
    {
        Skater,
        Goalie
    }
}
