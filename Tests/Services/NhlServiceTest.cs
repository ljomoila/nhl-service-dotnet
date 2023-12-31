using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Services;

namespace Tests.Services
{
    public class NhlServiceTest
    {
        private NhlService service;

        private Mock<INhlClient> client = new Mock<INhlClient>();
        private Mock<IMemoryCache> memoryCache = new Mock<IMemoryCache>();

        public NhlServiceTest()
        {
            service = new NhlService(client.Object, memoryCache.Object);
        }

        [Fact]
        public async void TestGetTeams_Success()
        {
            // Arrange
            List<Team> expectedTeams = TestHelper.GetTeams();
            client.Setup(x => x.GetTeams()).ReturnsAsync(expectedTeams);

            // Act
            List<Team> teams = await service.GetTeams();

            // Assert
            Assert.Equal(expectedTeams, teams);
        }

        [Fact]
        public async void TestGetTeams_FromCache()
        {
            // Arrange
            object expectedTeams = TestHelper.GetTeams();
            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedTeams))
                .Returns(true);

            // Act
            List<Team> teams = await new NhlService(
                client.Object,
                mockMemoryCache.Object
            ).GetTeams();

            // Assert
            Assert.Equal(expectedTeams, teams);
            client.Verify(client => client.GetTeams(), Times.Never());
        }

        [Fact]
        public void TestGetTeams_ThrowsNoTeams()
        {
            // Arrange
            client.Setup(x => x.GetTeams()).ReturnsAsync(new List<Team>());

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(async () => await service.GetTeams());

            // Assert
            Assert.Equal("No teams found", ex.Result.Message);
            Assert.Equal(HttpStatusCode.NoContent, ex.Result.StatusCode);
        }

        [Fact]
        public void TestGetTeams_ThrowsTeamsNull()
        {
            // Arrange
            List<Team>? expectedTeams = null;
            client.Setup(x => x.GetTeams()).ReturnsAsync(expectedTeams);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(async () => await service.GetTeams());

            // Assert
            Assert.Equal("No teams found", ex.Result.Message);
            Assert.Equal(HttpStatusCode.NoContent, ex.Result.StatusCode);
        }

        [Fact]
        public async void TestGetPlayer_Success()
        {
            // Arrange
            int id = 1;
            Player expectedPlayer = TestHelper.GetPlayer(id);
            client.Setup(x => x.GetPlayer(1)).ReturnsAsync(expectedPlayer);

            // Act
            Player player = await service.GetPlayer(id);

            // Assert
            Assert.Equal(expectedPlayer, player);
        }

        [Fact]
        public void TestGetPlayer_ThrowsPlayerNull()
        {
            // Arrange
            Player? expectedPlayer = null;
            client.Setup(x => x.GetPlayer(It.IsAny<int>())).ReturnsAsync(expectedPlayer);

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(
                async () => await service.GetPlayer(It.IsAny<int>())
            );

            // Assert
            Assert.Contains("No player found with id", ex.Result.Message);
            Assert.Equal(HttpStatusCode.NotFound, ex.Result.StatusCode);
        }

        public static IMemoryCache GetMemoryCache(object expectedValue)
        {
            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(true);
            return mockMemoryCache.Object;
        }
    }
}
