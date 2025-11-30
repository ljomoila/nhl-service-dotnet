using System.Net;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;
using System.Reflection.Metadata.Ecma335;

namespace nhl_service_dotnet.Services
{
    public class GameService : ServiceBase, IGameService
    {
        private readonly ILogger logger;
        private readonly INhlClient client;
        private readonly IMemoryCache cache;
        private readonly ITeamService teamService;
        private readonly ITeamRosterService teamRosterService;

        public GameService(
            ILogger<GameService> logger,
            INhlClient client,
            IMemoryCache cache,
            ITeamService teamService,
            ITeamRosterService teamRosterService,
            IOptions<AppSettings>? settings = null
        )
            : base(settings)
        {
            this.logger = logger;
            this.client = client;
            this.cache = cache;
            this.teamService = teamService;
            this.teamRosterService = teamRosterService;
        }

        public async Task<List<Game>> GetGames(string date)
        {
            string cacheKey = $"Games_{date}";
            if (cache.TryGetValue(cacheKey, out List<Game>? cached) && cached != null)
            {
                return cached;
            }

            // Preload teams and rosters for enrichment
            List<Team> teams = await this.teamService.GetTeams();
            var rosters = await this.teamRosterService.GetTeamsWithRosters();
            var rosterMap = rosters
                .Where(r => r.team != null && r.players != null)
                .ToDictionary(r => r.team!.id, r => r.players!);

            List<string> gamePaths = await client.GetScheduleGamesByDate(date);
            List<Game> games = new List<Game>();

            foreach (string path in gamePaths)
            {
                string feedCacheKey = $"LiveFeed_{path}";
                if (!cache.TryGetValue(feedCacheKey, out LiveFeed? liveFeed) || liveFeed == null)
                {
                    liveFeed = await client.GetLiveFeed(path);
                    if (liveFeed != null)
                    {
                        cache.Set(feedCacheKey, liveFeed, TimeSpan.FromMinutes(1));
                    }
                }

                if (liveFeed == null)
                {
                    throw new NhlException(
                        "No live feed found for path: " + path,
                        HttpStatusCode.NotFound
                    );
                }

                games.Add(await ConstructGame(liveFeed, teams, rosterMap));
            }

            cache.Set(cacheKey, games, TimeSpan.FromSeconds(this.settings.Cache.GamesSeconds));
            return games;
        }

        private async Task<Game> ConstructGame(LiveFeed feed, List<Team> teams, Dictionary<int, List<Player>> rosterMap)
        {
            JToken? homeToken = feed.liveData?.linescore?.teams?["home"];
            JToken? awayToken = feed.liveData?.linescore?.teams?["away"];

            GameTeam homeTeam = await BuildGameTeam(homeToken, teams, rosterMap);
            GameTeam awayTeam = await BuildGameTeam(awayToken, teams, rosterMap);

            string? detailedState = feed.gameData?.status?.detailedState;
            GameStatus status = detailedState switch
            {
                "FINAL" => GameStatus.Final,
                "OFF" => GameStatus.Scheduled,
                "FUT" => GameStatus.Scheduled,
                _ => GameStatus.InProgress
            };

            return new Game()
            {
                home = homeTeam,
                away = awayTeam,
                timeRemaining = feed.liveData?.linescore?.currentPeriodTimeRemaining,
                period = feed.liveData?.linescore?.currentPeriodOrdinal,
                status = status
            };
        }

        private async Task<GameTeam> BuildGameTeam(JToken? token, List<Team> teams, Dictionary<int, List<Player>> rosterMap)
        {
            if (token == null) return new GameTeam();

            int id = token.Value<int?>("id") ?? 0;
            Team? existing = teams.FirstOrDefault(t => t.id == id);
            List<GamePlayer>? players = await MapPlayers(token?["players"] as JArray, id, rosterMap);

            return new GameTeam()
            {
                id = id,
                name = existing?.name ?? token!.SelectToken("name.default")?.ToString()
                    ?? token!.SelectToken("commonName.default")?.ToString(),
                shortName = existing?.shortName ?? token!.SelectToken("commonName.default")?.ToString(),
                abbreviation = existing?.abbreviation ?? token!.Value<string?>("abbrev") ?? string.Empty,
                goals = token!.Value<int?>("score") ?? 0,
                players = players
            };
        }

        private Task<List<GamePlayer>?> MapPlayers(JArray? players, int teamId, Dictionary<int, List<Player>> rosterMap)
        {
            if (players == null) return Task.FromResult<List<GamePlayer>?>(null);

            var result = new List<GamePlayer>();

            foreach (JObject p in players
                .Where(p =>
                {
                    int goals = p.Value<int?>("goals") ?? 0;
                    int assists = p.Value<int?>("assists") ?? 0;
                    double savePctg = p.Value<double?>("savePercentage") ?? p.Value<double?>("savePctg") ?? 0;
                    int saves = p.Value<int?>("saves") ?? 0;

                    return goals > 0 || assists > 0 || savePctg > 0 || saves > 0;
                })
            )
            {
                string? positionToken = p.Value<string?>("position")
                    ?? p.Value<string?>("positionCode")
                    ?? p.Value<string?>("playerType");
                PlayerType type = (!string.IsNullOrWhiteSpace(positionToken) && positionToken.ToUpperInvariant().StartsWith("G"))
                    ? PlayerType.Goalie
                    : PlayerType.Skater;

                GamePlayer mapped;
                if (type == PlayerType.Goalie)
                {
                    string? saveShotsAgainst = p.Value<string?>("saveShotsAgainst");
                    double savePctPercent = p.Value<double?>("savePercentage") ?? p.Value<double?>("savePctg") ?? 0;

                    mapped = new GamePlayer()
                    {
                        id = p.Value<int?>("id") ?? 0,
                        fullName = p.Value<string?>("fullName"),
                        lastName = p.Value<string?>("lastName"),
                        position = type.ToString(),
                        goals = p.Value<int?>("goals") ?? 0,
                        assists = p.Value<int?>("assists") ?? 0,
                        points = p.Value<int?>("points")?.ToString(),
                        playerType = type,
                        saves = 0,
                        saveShotsAgainst = saveShotsAgainst,
                        savePercentage = Math.Round(savePctPercent * 100, 2)
                    };
                }
                else
                {
                    mapped = new GamePlayer()
                    {
                        id = p.Value<int?>("id") ?? 0,
                        fullName = p.Value<string?>("fullName"),
                        lastName = p.Value<string?>("lastName"),
                        position = type.ToString(),
                        goals = p.Value<int?>("goals") ?? 0,
                        assists = p.Value<int?>("assists") ?? 0,
                        points = p.Value<int?>("points")?.ToString(),
                        playerType = type
                    };
                }

                if (rosterMap.TryGetValue(teamId, out var roster))
                {
                    var details = roster.FirstOrDefault(r => r.id == mapped.id);

                    if (details != null)
                    {
                        mapped.nationality = details.nationality;
                        mapped.link = details.link;
                    }
                }

                result.Add(mapped);
            }

            var ordered = result
                .OrderBy(r => r.position == "Goalie" ? 1 : 0)
                .ThenByDescending(r => r.goals + r.assists)
                .ToList();

            return Task.FromResult<List<GamePlayer>?>(ordered);
        }
    }
}
