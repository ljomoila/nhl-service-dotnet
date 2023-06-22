using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace nhl_service_dotnet.Integrations
{
    public class NhlClient
    {
        private static readonly string apiUrl = "https://statsapi.web.nhl.com";
        private static readonly string apiPath = "/api/v1";

        private readonly HttpClient httpClient = new HttpClient();

        public NhlClient()
        {
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<Team[]?> GetTeams()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(
                    ConstructUrlWithPath("/teams")
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        "Failed to get teams",
                        null,
                        response.StatusCode
                    );
                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, Object>>(
                    response.Content.ReadAsStringAsync().Result
                );

                if (result == null || !result.ContainsKey("teams"))
                {
                    throw new Exception("No teams found from response");
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
