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

        private static readonly string apiUrl = "https://statsapi.web.nhl.com";
        private static readonly string apiPath = "/api/v1";

        private readonly HttpClient httpClient;

        public NhlClient(HttpClient httpClient, ILogger<NhlClient> logger)
        {
            this.logger = logger;

            this.httpClient = httpClient;
            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<List<Team>?> GetTeams()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(
                    ConstructUrlWithPath("/teams")
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new NhlException("Failed to get teams", response.StatusCode);
                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, Object>>(
                    response.Content.ReadAsStringAsync().Result
                );

                if (result == null || !result.ContainsKey("teams"))
                {
                    throw new NhlException("No teams found from response", HttpStatusCode.NotFound);
                }

                string teamsJsonString = JsonConvert.SerializeObject(result["teams"]);

                return JsonConvert.DeserializeObject<List<Team>>(teamsJsonString);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get teams, error: " + e.Message);
                throw;
            }
        }

        public async Task<Player?> GetPlayer(int id)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(
                    ConstructUrlWithPath("/people/" + id)
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new NhlException(
                        "Failed to get player with id: " + id,
                        response.StatusCode
                    );
                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, Object>>(
                    response.Content.ReadAsStringAsync().Result
                );

                if (result == null || !result.ContainsKey("people"))
                {
                    throw new NhlException(
                        "No player found from response with id: " + id,
                        HttpStatusCode.NotFound
                    );
                }

                string playerJson = JsonConvert.SerializeObject(result["people"]);

                return JsonConvert.DeserializeObject<List<Player>>(playerJson)[0];
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get player, error: " + e.Message);
                throw;
            }
        }

        public async Task<List<string>> GetScheduleGamesByDate(string date)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(
                    ConstructUrlWithPath("/schedule?date=" + date)
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new NhlException(
                        "Failed to get games for the date: " + date,
                        response.StatusCode
                    );
                }

                JObject? result = JsonConvert.DeserializeObject<JObject>(
                    response.Content.ReadAsStringAsync().Result
                );

                JArray games = (JArray)(result.SelectToken("dates[0].games"));

                List<string> gamePaths = new List<string>();
                foreach (JObject entry in games)
                {
                    gamePaths.Add(entry.SelectToken("link").ToString());
                }

                return gamePaths;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get scheduled games, error: " + e.Message);
                throw;
            }
        }

        public async Task<LiveFeed?> GetLiveFeed(string gamePath)
        {
            HttpResponseMessage response = await httpClient.GetAsync(
                ConstructUrlWithPath(gamePath)
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new NhlException(
                    "Failed to get live feed for the game: " + gamePath,
                    response.StatusCode
                );
            }

            return JsonConvert.DeserializeObject<LiveFeed>(
                response.Content.ReadAsStringAsync().Result
            );
        }

        private static string ConstructUrlWithPath(string path)
        {
            if (path.Contains(apiPath))
                return apiUrl + path;

            return apiUrl + apiPath + path;
        }
    }
}
