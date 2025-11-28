using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public class TeamService : ServiceBase, ITeamService
    {
        private readonly INhlClient client;
        private readonly IMemoryCache cache;
        private const string CacheKey = "Teams";

        public TeamService(INhlClient client, IMemoryCache cache, IOptions<AppSettings>? settings = null) : base(settings)
        {
            this.client = client;
            this.cache = cache;
        }

        public async Task<List<Team>> GetTeams()
        {
            if (cache.TryGetValue(CacheKey, out List<Team>? cached) && cached != null)
            {
                return cached;
            }

            List<Team>? teams = await client.GetTeams();

            if (teams == null || teams.Count == 0)
            {
                throw new NhlException("No teams found", HttpStatusCode.NoContent);
            }

            cache.Set(CacheKey, teams, TimeSpan.FromSeconds(this.settings.Cache.TeamsSeconds));
            return teams;
        }
    }
}
