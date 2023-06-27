using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Services
{
    public interface INhlService
    {
        Task<List<Team>> GetTeams();

        Task<Player> GetPlayer(int id);

        Task<List<Game>> GetGames(string date);
    }
}
