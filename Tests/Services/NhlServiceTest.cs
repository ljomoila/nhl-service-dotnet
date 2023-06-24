using System.Net;
using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Services;

namespace Tests.Services
{
    public class NhlServiceTest
    {
        private NhlService service;

        private Mock<INhlClient> client = new Mock<INhlClient>();

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
            Assert.Equal("No teams found", ex.Result.Message);
            Assert.Equal(HttpStatusCode.NoContent, ex.Result.StatusCode);
        }
    }
}
