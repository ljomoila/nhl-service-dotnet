using Moq;
using nhl_service_dotnet;
using nhl_service_dotnet.Controllers;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Services;

namespace Tests.Controllers
{
    public class NhlControllerTest
    {
        private Mock<ITeamService> teamService = new Mock<ITeamService>();
        private Mock<IPlayerService> playerService = new Mock<IPlayerService>();
        private Mock<IGameService> gameService = new Mock<IGameService>();

        [Fact]
        public async void TestGetTeams()
        {
            // Arrange
            List<Team> expectedTeams = TestHelper.GetTeams();
            teamService.Setup(x => x.GetTeams()).ReturnsAsync(expectedTeams);

            // Act
            List<Team> teams = await new NhlController(teamService.Object, playerService.Object, gameService.Object).GetTeams();

            // Assert
            Assert.Equal(expectedTeams, teams);
        }

        [Fact]
        public async void TestGetPlayer()
        {
            // Arrange
            int id = 1;
            Player expectedPlayer = TestHelper.GetPlayer(id);
            playerService.Setup(x => x.GetPlayer(1)).ReturnsAsync(expectedPlayer);

            // Act
            Player player = await new NhlController(teamService.Object, playerService.Object, gameService.Object).GetPlayer(id);

            // Assert
            Assert.Equal(expectedPlayer, player);
        }
    }
}
