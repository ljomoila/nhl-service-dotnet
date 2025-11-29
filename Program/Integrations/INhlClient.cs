using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Integrations
{
    public interface INhlClient
    {
        Task<List<Team>?> GetTeams();

        Task<Player?> GetPlayer(int id);

        Task<List<Player>?> GetRoster(string teamAbbreviation);

        Task<List<string>> GetScheduleGamesByDate(string date);

        Task<LiveFeed?> GetLiveFeed(string gamePath);
    }
}
