namespace nhl_service_dotnet.Services
{
    public interface INhlService
    {
        Task<List<Team>> GetTeams();
    }
}
