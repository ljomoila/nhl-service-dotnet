using System.Net.Http.Headers;
using Newtonsoft.Json;
using nhl_service_dotnet.Exceptions;
using System.Net;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;
using Newtonsoft.Json.Linq;

namespace nhl_service_dotnet.Integrations
{
    public class NhlClient : INhlClient
    {
        private readonly ILogger logger;

        private readonly string apiBase;
        private readonly string apiKeyName;
        private readonly string apiKeyValue;

        private readonly HttpClient httpClient;

        public NhlClient(HttpClient httpClient, ILogger<NhlClient> logger, Microsoft.Extensions.Options.IOptions<AppSettings>? settings = null)
        {
            this.logger = logger;

            this.httpClient = httpClient;
            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            this.apiBase = settings?.Value?.ApiBase ?? "https://api-web.nhle.com/v1";
            this.apiKeyName = settings?.Value?.ApiKeyName ?? string.Empty;
            this.apiKeyValue = settings?.Value?.ApiKeyValue ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(this.apiKeyName) && !string.IsNullOrWhiteSpace(this.apiKeyValue))
            {
                this.httpClient.DefaultRequestHeaders.Remove(this.apiKeyName);
                this.httpClient.DefaultRequestHeaders.Add(this.apiKeyName, this.apiKeyValue);
            }
        }

        public async Task<List<Team>?> GetTeams()
        {
            try
            {
                string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                JObject result = await this.DoGet($"{apiBase}/scoreboard/{today}");

                JArray gameDays = result["gamesByDate"] as JArray ?? new JArray();
                Dictionary<int, Team> teams = new Dictionary<int, Team>();

                foreach (JObject day in gameDays)
                {
                    JArray games = day["games"] as JArray ?? new JArray();
                    foreach (JObject game in games)
                    {
                        AddTeamFromToken(teams, game["homeTeam"]);
                        AddTeamFromToken(teams, game["awayTeam"]);
                    }
                }

                if (teams.Count == 0)
                {
                    throw new NhlException("No teams found from response", HttpStatusCode.NotFound);
                }

                return teams.Values.ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get teams");
                throw;
            }
        }

        public async Task<Player?> GetPlayer(int id)
        {
            try
            {
                JObject result = await DoGet($"{apiBase}/player/{id}/landing");

                Player player = new Player()
                {
                    id = result.Value<int>("playerId"),
                    fullName = result.SelectToken("fullName.default")?.ToString()
                        ?? $"{result.SelectToken("firstName.default")} {result.SelectToken("lastName.default")}",
                    lastName = result.SelectToken("lastName.default")?.ToString(),
                    nationality = result.Value<string>("birthCountry"),
                    link = $"/player/{id}/landing"
                };

                logger.LogError($"Fetched player: {player}");

                if (player.id == 0 || string.IsNullOrWhiteSpace(player.fullName))
                {
                    throw new NhlException($"No player found from response with id {id}", HttpStatusCode.NotFound);
                }

                return player;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get player");
                throw;
            }
        }

        public async Task<List<string>> GetScheduleGamesByDate(string date)
        {
            try
            {
                JObject result = await this.DoGet($"{apiBase}/schedule/{date}");

                JArray gameWeek = result["gameWeek"] as JArray ?? new JArray();

                List<string> gamePaths = new List<string>();
                foreach (JObject day in gameWeek)
                {
                    if (!string.Equals(day.Value<string>("date"), date, StringComparison.OrdinalIgnoreCase))
                        continue;

                    JArray games = day["games"] as JArray ?? new JArray();
                    string[] allowedStates = new[] { "LIVE", "CRIT", "FINAL", "FUT", "PRE" };

                    foreach (JObject game in games)
                    {
                        // string? state = game.Value<string>("gameState");
                        // if (string.IsNullOrWhiteSpace(state) || !allowedStates.Contains(state.ToUpperInvariant()))
                        // {
                        //     continue;
                        // }

                        string? id = game.Value<string>("id");
                        string? link = game.Value<string>("gameCenterLink");

                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            gamePaths.Add(id);
                        }
                        else if (!string.IsNullOrWhiteSpace(link))
                        {
                            gamePaths.Add(link);
                        }
                    }
                }

                return gamePaths;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get scheduled games");
                throw;
            }
        }

        public async Task<LiveFeed?> GetLiveFeed(string gamePath)
        {
            try
            {
                string gameId = ExtractGameId(gamePath);
                JObject result = await this.DoGet($"{apiBase}/gamecenter/{gameId}/boxscore");

                JObject teams = new JObject
                {
                    ["home"] = BuildTeamToken(result["homeTeam"], result.SelectToken("playerByGameStats.homeTeam")),
                    ["away"] = BuildTeamToken(result["awayTeam"], result.SelectToken("playerByGameStats.awayTeam"))
                };

                return new LiveFeed()
                {
                    gameData = new GameData()
                    {
                        status = new Status() { detailedState = result.Value<string>("gameState") }
                    },
                    liveData = new LiveData()
                    {
                        linescore = new LineScore()
                        {
                            currentPeriodOrdinal = result.SelectToken("periodDescriptor.number")?.ToString(),
                            currentPeriodTimeRemaining = result.SelectToken("clock.timeRemaining")?.ToString(),
                            teams = teams
                        },
                        boxscore = new BoxScore() { teams = teams }
                    }
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get live feed");
                throw;
            }
        }

        private async Task<JObject> DoGet(string url)
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new NhlException(
                    $"Invalid response, url: {url}, status: {response.StatusCode}",
                    response.StatusCode
                );
            }

            string content = await response.Content.ReadAsStringAsync();
            JObject? result = JsonConvert.DeserializeObject<JObject>(content);

            if (result == null)
            {
                throw new NhlException(
                    "Could not parse result from response, url: " + url,
                    HttpStatusCode.NoContent
                );
            }

            return result;
        }

        private static void AddTeamFromToken(Dictionary<int, Team> teams, JToken? token)
        {
            if (token == null) return;

            int id = token.Value<int?>("id") ?? 0;
            if (id == 0 || teams.ContainsKey(id)) return;

            Team team = new Team()
            {
                id = id,
                name = token.SelectToken("name.default")?.ToString()
                    ?? token.SelectToken("commonName.default")?.ToString(),
                shortName = token.SelectToken("commonName.default")?.ToString(),
                abbreviation = token.Value<string>("abbrev"),
                link = token.Value<string>("link") ?? token.Value<string>("teamLink")
            };

            teams.Add(id, team);
        }

        private JObject BuildTeamToken(JToken? teamToken, JToken? statsToken)
        {
            JObject team = new JObject();
            if (teamToken != null)
            {
                foreach (JProperty prop in teamToken.Children<JProperty>())
                {
                    team[prop.Name] = prop.Value;
                }
            }

            team["players"] = BuildPlayersArray(statsToken);
            return team;
        }

        private JArray BuildPlayersArray(JToken? statsToken)
        {
            JArray players = new JArray();
            if (statsToken == null) return players;

            AppendPlayers(players, statsToken["forwards"], PlayerType.Skater);
            AppendPlayers(players, statsToken["defense"], PlayerType.Skater);
            AppendPlayers(players, statsToken["goalies"], PlayerType.Goalie);

            return players;
        }

        private void AppendPlayers(JArray target, JToken? group, PlayerType type)
        {
            if (group == null) return;
            foreach (JObject player in group.Children<JObject>())
            {
                JObject mapped = new JObject
                {
                    ["id"] = player.Value<int?>("playerId") ?? 0,
                    ["fullName"] = player.SelectToken("name.default"),
                    ["lastName"] = player.SelectToken("lastName.default") ?? player.SelectToken("name.default"),
                    ["goals"] = player.Value<int?>("goals") ?? 0,
                    ["assists"] = player.Value<int?>("assists") ?? 0,
                    ["points"] = player.Value<int?>("points") ?? 0,
                    ["playerType"] = type.ToString(),
                    ["position"] = player.Value<string?>("position")
                };

                if (type == PlayerType.Goalie)
                {
                    mapped["saves"] = player.Value<int?>("saves") ?? 0;
                    mapped["savePercentage"] = player.Value<double?>("savePctg") ?? 0;
                }

                target.Add(mapped);
            }
        }

        private static string ExtractGameId(string gamePath)
        {
            string trimmed = gamePath.Trim('/');
            if (int.TryParse(trimmed, out _))
            {
                return trimmed;
            }

            string[] parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? trimmed : parts[parts.Length - 1];
        }
    }
}
