using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public interface ITeamRosterService
    {
        Task<List<TeamRoster>> GetTeamsWithRosters();
        Task RefreshTeamsAndRosters();
    }
}
