namespace nhl_service_dotnet.Integrations
{
    public interface INhlClient
    {
        Task<List<Team>?> GetTeams();
    }
}
