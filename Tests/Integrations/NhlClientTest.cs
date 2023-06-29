using System.Net;
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

        [Fact]
        public async void TestGetPlayer_Success()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"people\":[{\"id\":1,\"fullName\":\"Test Player\",\"link\":\"/api/v1/people/1\",\"firstName\":\"Test\",\"lastName\":\"Player\",\"primaryNumber\":\"37\",\"birthDate\":\"1996-04-01\",\"currentAge\":27,\"birthCity\":\"Markham\",\"birthStateProvince\":\"ON\",\"birthCountry\":\"FIN\",\"nationality\":\"FIN\",\"height\":\"6' 2\\\"\",\"weight\":198,\"active\":true,\"alternateCaptain\":false,\"captain\":false,\"rookie\":false,\"shootsCatches\":\"L\",\"rosterStatus\":\"Y\",\"currentTeam\":{\"id\":22,\"name\":\"Edmonton Oilers\",\"link\":\"/api/v1/teams/22\"},\"primaryPosition\":{\"code\":\"L\",\"name\":\"Left Wing\",\"type\":\"Forward\",\"abbreviation\":\"LW\"}}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            Player? player = await CreateClient(httpClient).GetPlayer(1);

            // Assert
            Assert.NotNull(player);
            Assert.Equal("Test Player", player.fullName);
        }

        [Fact]
        public void TestGetPlayer_ThrowsInvalidResponse()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"teamss\":[]}";
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
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"metaData\":{\"timeStamp\":\"20230629_081825\"},\"wait\":10,\"dates\":[{\"date\":\"2023-05-08\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"games\":[{\"gamePk\":2022030243,\"link\":\"/api/v1/game/2022030243/feed/live\",\"gameType\":\"P\",\"season\":\"20222023\",\"gameDate\":\"2023-05-09T00:30:00Z\",\"status\":{\"abstractGameState\":\"Final\",\"codedGameState\":\"7\",\"detailedState\":\"Final\",\"statusCode\":\"7\",\"startTimeTBD\":false},\"teams\":{\"away\":{\"leagueRecord\":{\"wins\":6,\"losses\":2,\"type\":\"league\"},\"score\":5,\"team\":{\"id\":54,\"name\":\"Vegas Golden Knights\",\"link\":\"/api/v1/teams/54\"}},\"home\":{\"leagueRecord\":{\"wins\":5,\"losses\":4,\"type\":\"league\"},\"score\":1,\"team\":{\"id\":22,\"name\":\"Edmonton Oilers\",\"link\":\"/api/v1/teams/22\"}}},\"venue\":{\"id\":5100,\"name\":\"Rogers Place\",\"link\":\"/api/v1/venues/5100\"},\"content\":{\"link\":\"/api/v1/game/2022030243/content\"}}],\"events\":[],\"matches\":[]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<string> gamePaths = await CreateClient(httpClient)
                .GetScheduleGamesByDate("2023-05-08");

            // Assert
            Assert.NotNull(gamePaths);
            Assert.Equal(1, gamePaths.Count);
        }

        [Fact]
        public async void TestGetScheduleGamesByDate_NoGames()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"metaData\":{\"timeStamp\":\"20230629_081825\"},\"wait\":10,\"dates\":[{\"date\":\"2023-05-08\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"games\":[],\"events\":[],\"matches\":[]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            List<string> gamePaths = await CreateClient(httpClient)
                .GetScheduleGamesByDate("2023-05-08");

            // Assert
            Assert.NotNull(gamePaths);
            Assert.Equal(0, gamePaths.Count);
        }

        [Fact]
        public async void TestGetLiveFeed_Success()
        {
            // Arrange
            string response =
                "{\"copyright\":\"NHL and the NHL Shield are registered trademarks of the National Hockey League. NHL and NHL team marks are the property of the NHL and its teams. © NHL 2023. All Rights Reserved.\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"metaData\":{\"timeStamp\":\"20230629_081825\"},\"wait\":10,\"dates\":[{\"date\":\"2023-05-08\",\"totalItems\":1,\"totalEvents\":0,\"totalGames\":1,\"totalMatches\":0,\"games\":[{\"gamePk\":2022030243,\"link\":\"/api/v1/game/2022030243/feed/live\",\"gameType\":\"P\",\"season\":\"20222023\",\"gameDate\":\"2023-05-09T00:30:00Z\",\"status\":{\"abstractGameState\":\"Final\",\"codedGameState\":\"7\",\"detailedState\":\"Final\",\"statusCode\":\"7\",\"startTimeTBD\":false},\"teams\":{\"away\":{\"leagueRecord\":{\"wins\":6,\"losses\":2,\"type\":\"league\"},\"score\":5,\"team\":{\"id\":54,\"name\":\"Vegas Golden Knights\",\"link\":\"/api/v1/teams/54\"}},\"home\":{\"leagueRecord\":{\"wins\":5,\"losses\":4,\"type\":\"league\"},\"score\":1,\"team\":{\"id\":22,\"name\":\"Edmonton Oilers\",\"link\":\"/api/v1/teams/22\"}}},\"venue\":{\"id\":5100,\"name\":\"Rogers Place\",\"link\":\"/api/v1/venues/5100\"},\"content\":{\"link\":\"/api/v1/game/2022030243/content\"}}],\"events\":[],\"matches\":[]}]}";
            HttpClient httpClient = TestHelper.CreateHttpClient(response, HttpStatusCode.OK);

            // Act
            LiveFeed? feed = await CreateClient(httpClient)
                .GetLiveFeed("/api/v1/game/2022030243/feed/live");

            // Assert
            Assert.NotNull(feed);
            //Assert.Equal(1, gamePaths.Count);
        }

        private NhlClient CreateClient(HttpClient client)
        {
            return new NhlClient(client, (ILogger<NhlClient>)logger);
        }
    }
}
