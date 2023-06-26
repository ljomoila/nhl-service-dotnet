using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public interface INhlService
    {
        Task<List<Team>> GetTeams();

        Task<Player> GetPlayer(int id);
    }
}
