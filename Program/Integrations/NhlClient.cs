using System.Net.Http.Headers;
using Newtonsoft.Json;
using nhl_service_dotnet.Exceptions;
using System.Net;

namespace nhl_service_dotnet.Integrations
{
    public class NhlClient
    {
        private static readonly string apiUrl = "https://statsapi.web.nhl.com";
        private static readonly string apiPath = "/api/v1";

        private readonly HttpClient _httpClient;

        public NhlClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<Team[]?> GetTeams()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(
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

                return JsonConvert.DeserializeObject<Team[]>(teamsJsonString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string ConstructUrlWithPath(string path)
        {
            if (path.Contains(apiPath))
                return apiUrl + path;

            return apiUrl + apiPath + path;
        }
    }
}