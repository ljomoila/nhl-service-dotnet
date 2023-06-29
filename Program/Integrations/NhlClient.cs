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
                JObject result = await this.DoGet(ConstructUrlWithPath("/teams"));

                if (!result.ContainsKey("teams"))
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
                JObject? result = await this.DoGet(ConstructUrlWithPath("/people/" + id));

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
                JObject? result = await this.DoGet(ConstructUrlWithPath("/schedule?date=" + date));

                JArray games = (JArray)(result.SelectToken("dates[0].games"));

                if (games == null)
                {
                    return new List<string>();
                }

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
            try
            {
                JObject? result = await this.DoGet(ConstructUrlWithPath(gamePath));

                return result.ToObject<LiveFeed>();
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get live feed, error: " + e.Message);
                throw;
            }
        }

        private async Task<JObject> DoGet(string url)
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new NhlException("Invalid response from " + url, response.StatusCode);
            }

            JObject? result = JsonConvert.DeserializeObject<JObject>(
                response.Content.ReadAsStringAsync().Result
            );

            if (result == null)
            {
                throw new NhlException(
                    "Could not parse result from response, url: " + url,
                    HttpStatusCode.NoContent
                );
            }

            return result;
        }

        private static string ConstructUrlWithPath(string path)
        {
            if (path.Contains(apiPath))
                return apiUrl + path;

            return apiUrl + apiPath + path;
        }
    }
}
