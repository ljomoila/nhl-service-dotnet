using Newtonsoft.Json.Linq;

namespace nhl_service_dotnet.Models.Game
{
    public class LineScore
    {
        public string? currentPeriodOrdinal { get; set; }
        public string? currentPeriodTimeRemaining { get; set; }
        public string? periodType { get; set; }
        public string? gameState { get; set; }
        public JObject? teams { get; set; }
    }
}
