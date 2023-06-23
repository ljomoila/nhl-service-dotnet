using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Services;

namespace Tests.Services
{
    public class NhlServiceTest
    {
        private readonly Mock<NhlClient> client = new Mock<NhlClient>();
        private NhlService service;

        public NhlServiceTest()
        {
            service = new NhlService(client.Object);
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
        public void TestGetTeams_ThrowsNoTeams()
        {
            // Arrange
            client.Setup(x => x.GetTeams()).ReturnsAsync(new List<Team>());

            // Act
            var ex = Assert.ThrowsAsync<NhlException>(async () => await service.GetTeams());

            // Assert
            Assert.Equal("Failed to get teams", ex.Result.Message);
        }
    }
}
