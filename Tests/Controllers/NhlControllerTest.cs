using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Controllers;
using nhl_service_dotnet.Services;

namespace Tests.Controllers
{
    public class NhlControllerTest
    {
        private Mock<INhlService> service = new Mock<INhlService>();

        [Fact]
        public async void TestGetTeams()
        {
            // Arrange
            List<Team> expectedTeams = TestHelper.GetTeams();
            service.Setup(x => x.GetTeams()).ReturnsAsync(expectedTeams);

            // Act
            List<Team> teams = await new NhlController(service.Object).GetTeams();

            // Assert
            Assert.Equal(expectedTeams, teams);
        }
    }
}
