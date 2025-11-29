using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public class TeamRosterService : ServiceBase, ITeamRosterService
    {
        private readonly INhlClient nhlClient;
        private readonly ITeamService teamService;
        private readonly IMemoryCache cache;
        private readonly ILogger<TeamRosterService> logger;
        private const string CacheKey = "TeamsWithRosters";

        public TeamRosterService(
            INhlClient nhlClient,
            ITeamService teamService,
            IMemoryCache cache,
            ILogger<TeamRosterService> logger,
            IOptions<AppSettings>? settings = null
        ) : base(settings)
        {
            this.nhlClient = nhlClient;
            this.teamService = teamService;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<List<TeamRoster>> GetTeamsWithRosters()
        {
            if (cache.TryGetValue(CacheKey, out List<TeamRoster>? cached) && cached != null)
            {
                return cached;
            }

            List<Team> teams = await teamService.GetTeams();
            var bag = new ConcurrentBag<TeamRoster>();

            await Task.WhenAll(
                teams.Select(async team =>
                {
                    try
                    {
                        string abbrev = team.abbreviation ?? string.Empty;
                        List<Player>? roster = await nhlClient.GetRoster(abbrev);
                        bag.Add(new TeamRoster { team = team, players = roster ?? new List<Player>() });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to load roster for team {Abbrev}", team.abbreviation);
                        bag.Add(new TeamRoster { team = team, players = new List<Player>() });
                    }
                })
            );

            List<TeamRoster> result = bag.ToList();
            cache.Set(CacheKey, result, TimeSpan.FromSeconds(this.settings.Cache.RostersSeconds));
            return result;
        }
    }
}
