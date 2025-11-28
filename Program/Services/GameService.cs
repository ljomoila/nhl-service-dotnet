using System.Net;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Services
{
    public class GameService : ServiceBase, IGameService
    {
        private readonly INhlClient client;
        private readonly IMemoryCache cache;
        private readonly ITeamService teamService;
        private readonly IPlayerService playerService;

        public GameService(
            INhlClient client,
            IMemoryCache cache,
            ITeamService teamService,
            IPlayerService playerService,
            IOptions<AppSettings>? settings = null
        )
            : base(settings)
        {
            this.client = client;
            this.cache = cache;
            this.teamService = teamService;
            this.playerService = playerService;
        }

        public async Task<List<Game>> GetGames(string date)
        {
            string cacheKey = $"Games_{date}";
            if (cache.TryGetValue(cacheKey, out List<Game>? cached) && cached != null)
            {
                return cached;
            }

            List<string> gamePaths = await client.GetScheduleGamesByDate(date);
            List<Game> games = new List<Game>();

            foreach (string path in gamePaths)
            {
                LiveFeed? liveFeed = await client.GetLiveFeed(path);

                if (liveFeed == null)
                {
                    throw new NhlException(
                        "No live feed found for path: " + path,
                        HttpStatusCode.NotFound
                    );
                }

                games.Add(await ConstructGame(liveFeed));
            }

            cache.Set(cacheKey, games, TimeSpan.FromSeconds(this.settings.Cache.GamesSeconds));
            return games;
        }

        private async Task<Game> ConstructGame(LiveFeed feed)
        {
            List<Team> teams = await this.teamService.GetTeams();

            JToken? homeToken = feed.liveData?.linescore?.teams?["home"];
            JToken? awayToken = feed.liveData?.linescore?.teams?["away"];

            GameTeam homeTeam = await BuildGameTeam(homeToken, teams);
            GameTeam awayTeam = await BuildGameTeam(awayToken, teams);

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

        private async Task<GameTeam> BuildGameTeam(JToken? token, List<Team> teams)
        {
            if (token == null) return new GameTeam();

            int id = token.Value<int?>("id") ?? 0;
            Team? existing = teams.FirstOrDefault(t => t.id == id);
            List<GamePlayer>? players = await MapPlayers(token["players"] as JArray);

            return new GameTeam()
            {
                id = id,
                name = existing?.name ?? token.SelectToken("name.default")?.ToString()
                    ?? token.SelectToken("commonName.default")?.ToString(),
                shortName = existing?.shortName ?? token.SelectToken("commonName.default")?.ToString(),
                abbreviation = existing?.abbreviation ?? token.Value<string>("abbrev"),
                goals = token.Value<int?>("score") ?? 0,
                players = players
            };
        }

        private async Task<List<GamePlayer>?> MapPlayers(JArray? players)
        {
            if (players == null) return null;

            var result = new List<GamePlayer>();

            foreach (JObject p in players
                .Where(p =>
                {
                    int goals = p.Value<int?>("goals") ?? 0;
                    int assists = p.Value<int?>("assists") ?? 0;
                    return goals > 0 || assists > 0;
                })
            )
            {
                PlayerType type = Enum.TryParse<PlayerType>(p.Value<string?>("playerType"), true, out var parsed)
                    ? parsed
                    : PlayerType.Skater;

                GamePlayer mapped;
                if (type == PlayerType.Goalie)
                {
                    mapped = new GameGoalie()
                    {
                        id = p.Value<int?>("id") ?? 0,
                        fullName = p.Value<string?>("fullName"),
                        lastName = p.Value<string?>("lastName"),
                        position = type.ToString(),
                        goals = p.Value<int?>("goals") ?? 0,
                        assists = p.Value<int?>("assists") ?? 0,
                        points = p.Value<int?>("points")?.ToString(),
                        playerType = type,
                        saves = p.Value<int?>("saves") ?? 0,
                        savePercentage = p.Value<double?>("savePercentage") ?? 0
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

                int goals = mapped.goals;
                int assists = mapped.assists;
                if (mapped.id != 0 && (goals > 0 || assists > 0))
                {
                    try
                    {
                        Player details = await playerService.GetPlayer(mapped.id);
                        mapped.nationality = details?.nationality;
                        mapped.link = details?.link;
                    }
                    catch
                    {
                        // swallow enrichment failures; base game stats still returned
                    }
                }

                result.Add(mapped);
            }

            return result;
        }
    }
}
