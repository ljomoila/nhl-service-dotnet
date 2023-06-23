using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Controllers;
using nhl_service_dotnet.Services;

namespace Tests.Controllers
{
    public class NhlControllerTest
    {
        private readonly Mock<INhlService> service = new Mock<INhlService>();

        [Fact]
        public async void TestGetTeams()
        {
            // Arrange
            List<Team> expectedTeams = GetTeams();
            service.Setup(x => x.GetTeams()).ReturnsAsync(expectedTeams);

            // Act
            // TODO: fix ControllerBase problem
            List<Team> teams = new List<Team>(); //await new NhlController(service.Object).GetTeams();

            // Assert
            Assert.Equal(expectedTeams, teams);
        }

        private static List<Team> GetTeams()
        {
            List<Team> teams = new List<Team>();

            Team team = new Team();
            team.id = 0;
            team.name = "Test Team";

            teams.Add(team);

            return teams;
        }
    }
}
