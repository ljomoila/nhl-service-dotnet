using System.Net;
using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using Microsoft.Extensions.Logging;

namespace Tests.Integrations
{
    public class NhlClientTest
    {
        private ILogger logger = Mock.Of<ILogger<NhlClient>>();

        [Fact]
        public async void TestGetTeams_Success()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"teams\":[{\"id\":1,\"name\":\"New Jersey Devils\",\"link\":\"/api/v1/teams/1\",\"venue\":{\"name\":\"Prudential Center\",\"link\":\"/api/v1/venues/null\",\"city\":\"Newark\",\"timeZone\":{\"id\":\"America/New_York\",\"offset\":-4,\"tz\":\"EDT\"}},\"abbreviation\":\"NJD\",\"teamName\":\"Devils\",\"locationName\":\"New Jersey\",\"firstYearOfPlay\":\"1982\",\"division\":{\"id\":18,\"name\":\"Metropolitan\",\"nameShort\":\"Metro\",\"link\":\"/api/v1/divisions/18\",\"abbreviation\":\"M\"},\"conference\":{\"id\":6,\"name\":\"Eastern\",\"link\":\"/api/v1/conferences/6\"},\"franchise\":{\"franchiseId\":23,\"teamName\":\"Devils\",\"link\":\"/api/v1/franchises/23\"},\"shortName\":\"New Jersey\",\"officialSiteUrl\":\"http://www.newjerseydevils.com/\",\"franchiseId\":23,\"active\":true},{\"id\":2,\"name\":\"New York Islanders\",\"link\":\"/api/v1/teams/2\",\"venue\":{\"name\":\"UBS Arena\",\"link\":\"/api/v1/venues/null\",\"city\":\"Elmont\",\"timeZone\":{\"id\":\"America/New_York\",\"offset\":-4,\"tz\":\"EDT\"}},\"abbreviation\":\"NYI\",\"teamName\":\"Islanders\",\"locationName\":\"New York\",\"firstYearOfPlay\":\"1972\",\"division\":{\"id\":18,\"name\":\"Metropolitan\",\"nameShort\":\"Metro\",\"link\":\"/api/v1/divisions/18\",\"abbreviation\":\"M\"},\"conference\":{\"id\":6,\"name\":\"Eastern\",\"link\":\"/api/v1/conferences/6\"},\"franchise\":{\"franchiseId\":22,\"teamName\":\"Islanders\",\"link\":\"/api/v1/franchises/22\"},\"shortName\":\"NY Islanders\",\"officialSiteUrl\":\"http://www.newyorkislanders.com/\",\"franchiseId\":22,\"active\":true}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<Team>? teams = await CreateClient(httpClient).GetTeams();

            // Assert
            Assert.NotNull(teams);
            Assert.Equal(2, teams.Count);
            Assert.Equal("New Jersey Devils", teams[0].name);
        }

        [Fact]
        public void TestGetTeams_ThrowsInvalidStatus()
        {
            // Arrange
            HttpStatusCode expectedStatus = HttpStatusCode.BadRequest;
            HttpClient httpClient = TestHelper.CreateHttpClient("{}", expectedStatus);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(
                async () => await CreateClient(httpClient).GetTeams()
            );

            // Assert
            Assert.Equal("Failed to get teams", ex.Result.Message);
            Assert.Equal(expectedStatus, ex.Result.StatusCode);
        }

        [Fact]
        public void TestGetTeams_ThrowsInvalidResponse()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"teamss\":[]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(
                async () => await CreateClient(httpClient).GetTeams()
            );

            // Assert
            Assert.Equal("No teams found from response", ex.Result.Message);
        }

        private NhlClient CreateClient(HttpClient client)
        {
            return new NhlClient(client, (ILogger<NhlClient>)logger);
        }
    }
}
