using System.Net;
using Moq;
using Moq.Protected;
using nhl_service_dotnet;

namespace Tests
{
    public class TestHelper
    {
        public static List<Team> GetTeams()
        {
            List<Team> teams = new List<Team>();

            Team team = new Team();
            team.id = 0;
            team.name = "Test Team";

            teams.Add(team);

            return teams;
        }

        public static HttpClient CreateHttpClient(string response, HttpStatusCode status)
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage()
                    {
                        StatusCode = status,
                        Content = new StringContent(response),
                    }
                )
                .Verifiable();

            return new HttpClient(messageHandlerMock.Object);
        }
    }
}
