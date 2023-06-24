using System.Net;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;

namespace nhl_service_dotnet.Services
{
    public class NhlService : INhlService
    {
        private readonly INhlClient client;

        public NhlService(INhlClient client)
        {
            this.client = client;
        }

        public async Task<List<Team>> GetTeams()
        {
            List<Team>? teams = await client.GetTeams();

            if (teams == null || teams.Count == 0)
            {
                throw new NhlException("No teams found", HttpStatusCode.NoContent);
            }

            return teams;
        }
    }
}
