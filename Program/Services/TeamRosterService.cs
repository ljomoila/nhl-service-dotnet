using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nhl_service_dotnet.Data;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public class TeamRosterService : ServiceBase, ITeamRosterService
    {
        private readonly INhlClient nhlClient;
        private readonly ITeamService teamService;
        private readonly IMemoryCache cache;
        private readonly NhlDbContext dbContext;
        private readonly ILogger<TeamRosterService> logger;
        private const string CacheKey = "TeamsWithRosters";

        public TeamRosterService(
            INhlClient nhlClient,
            ITeamService teamService,
            IMemoryCache cache,
            ILogger<TeamRosterService> logger,
            NhlDbContext dbContext,
            IOptions<AppSettings>? settings = null
        ) : base(settings)
        {
            this.nhlClient = nhlClient;
            this.teamService = teamService;
            this.cache = cache;
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public async Task<List<TeamRoster>> GetTeamsWithRosters()
        {
            if (cache.TryGetValue(CacheKey, out List<TeamRoster>? cached) && cached != null)
            {
                return cached;
            }

            List<Team> teams = await dbContext.Teams.AsNoTracking().ToListAsync();

            // If DB empty, refresh from API
            if (teams.Count == 0)
            {
                await RefreshTeamsAndRosters();
                teams = await dbContext.Teams.AsNoTracking().ToListAsync();
            }

            var result = new List<TeamRoster>();
            foreach (var team in teams)
            {
                var players = await dbContext.Players
                    .AsNoTracking()
                    .Where(p => EF.Property<int?>(p, "TeamId") == team.id)
                    .ToListAsync();

                result.Add(new TeamRoster { team = team, players = players });
            }

            cache.Set(CacheKey, result, TimeSpan.FromSeconds(this.settings.Cache.RostersSeconds));
            return result;
        }

        public async Task RefreshTeamsAndRosters()
        {
            List<Team> teams = await teamService.GetTeams();
            var rosterBag = new ConcurrentBag<(Team team, List<Player> players)>();

            await Task.WhenAll(
                teams.Select(async team =>
                {
                    try
                    {
                        string abbrev = team.abbreviation ?? string.Empty;
                        List<Player>? roster = await nhlClient.GetRoster(abbrev);
                        rosterBag.Add((team, roster ?? new List<Player>()));
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to load roster for team {Abbrev}", team.abbreviation);
                        rosterBag.Add((team, new List<Player>()));
                    }
                })
            );

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                dbContext.Players.RemoveRange(dbContext.Players);
                dbContext.Teams.RemoveRange(dbContext.Teams);
                await dbContext.SaveChangesAsync();

                foreach (var (team, players) in rosterBag)
                {
                    dbContext.Teams.Add(team);
                    foreach (var player in players)
                    {
                        var entry = dbContext.Players.Add(player);
                        entry.Property("TeamId").CurrentValue = team.id;
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                cache.Remove(CacheKey);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to refresh teams and rosters");
                throw;
            }
        }
    }
}
