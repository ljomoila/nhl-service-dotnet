using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Integrations
{
    public interface INhlClient
    {
        Task<List<Team>?> GetTeams();

        Task<Player?> GetPlayer(int id);
    }
}
