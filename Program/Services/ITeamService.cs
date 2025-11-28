using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public interface ITeamService
    {
        Task<List<Team>> GetTeams();
    }
}
