using System.Net;
using System.Linq;
using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using Microsoft.Extensions.Logging;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

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
                "{\"gamesByDate\":[{\"date\":\"2024-10-01\",\"games\":[{\"id\":2024010071,\"homeTeam\":{\"id\":5,\"name\":{\"default\":\"Pittsburgh Penguins\"},\"commonName\":{\"default\":\"Penguins\"},\"abbrev\":\"PIT\"},\"awayTeam\":{\"id\":17,\"name\":{\"default\":\"Detroit Red Wings\"},\"commonName\":{\"default\":\"Red Wings\"},\"abbrev\":\"DET\"}}]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<Team>? teams = await CreateClient(httpClient).GetTeams();

            // Assert
            Assert.NotNull(teams);
            Assert.Equal(2, teams.Count);
            Assert.Contains(teams, t => t.abbreviation == "PIT" && t.name == "Pittsburgh Penguins");
            Assert.Contains(teams, t => t.abbreviation == "DET" && t.name == "Detroit Red Wings");
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
            Assert.Equal(expectedStatus, ex.Result.StatusCode);
            Assert.Contains("Invalid response", ex.Result.Message);
        }

        [Fact]
        public void TestGetTeams_ThrowsInvalidResponse()
        {
            // Arrange
            string response = "{\"gamesByDate\":[]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(
                async () => await CreateClient(httpClient).GetTeams()
            );

            // Assert
            Assert.Equal("No teams found from response", ex.Result.Message);
        }

        [Fact]
        public async void TestGetPlayer_Success()
        {
            // Arrange
            string response =
                "{\"playerId\":8478402,\"firstName\":{\"default\":\"Connor\"},\"lastName\":{\"default\":\"McDavid\"},\"birthCountry\":\"CAN\"}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            Player? player = await CreateClient(httpClient).GetPlayer(8478402);

            // Assert
            Assert.NotNull(player);
            Assert.Equal("Connor McDavid", player.fullName);
            Assert.Equal("CAN", player.nationality);
        }

        [Fact]
        public void TestGetPlayer_ThrowsInvalidResponse()
        {
            // Arrange
            string response = "{}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(
                async () => await CreateClient(httpClient).GetPlayer(It.IsAny<int>())
            );

            // Assert
            Assert.Contains("No player found from response with id", ex.Result.Message);
        }

        [Fact]
        public async void TestGetScheduleGamesByDate_Success()
        {
            // Arrange
            string response =
                "{\"gameWeek\":[{\"date\":\"2024-10-01\",\"games\":[{\"id\":2024010071,\"gameState\":\"LIVE\",\"gameCenterLink\":\"/gamecenter/det-vs-pit/2024/10/01/2024010071\"}]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<string> gamePaths = await CreateClient(httpClient)
                .GetScheduleGamesByDate("2024-10-01");

            // Assert
            Assert.NotNull(gamePaths);
            Assert.Equal(1, gamePaths.Count);
        }

        [Fact]
        public async void TestGetScheduleGamesByDate_NoGames()
        {
            // Arrange
            string response =
                "{\"gameWeek\":[{\"date\":\"2024-10-01\",\"games\":[]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<string> gamePaths = await CreateClient(httpClient)
                .GetScheduleGamesByDate("2024-10-01");

            // Assert
            Assert.NotNull(gamePaths);
            Assert.Equal(0, gamePaths.Count);
        }

        [Fact]
        public async void TestGetLiveFeed_Success()
        {
            // Arrange
            string response =
                "{\"gameState\":\"FINAL\",\"periodDescriptor\":{\"number\":3},\"clock\":{\"timeRemaining\":\"00:00\"},\"homeTeam\":{\"id\":5,\"abbrev\":\"PIT\",\"score\":1},\"awayTeam\":{\"id\":17,\"abbrev\":\"DET\",\"score\":2}}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            LiveFeed? feed = await CreateClient(httpClient)
                .GetLiveFeed("2024010071");

            // Assert
            Assert.NotNull(feed);
            Assert.Equal("FINAL", feed?.gameData?.status?.detailedState);
        }

        private NhlClient CreateClient(HttpClient client)
        {
            return new NhlClient(client, (ILogger<NhlClient>)logger, Microsoft.Extensions.Options.Options.Create(new AppSettings()));
        }
    }
}
