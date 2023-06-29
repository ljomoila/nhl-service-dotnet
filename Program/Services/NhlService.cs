using System.Net;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Services
{
    public class NhlService : INhlService
    {
        private readonly INhlClient client;

        public NhlService(INhlClient client)
        {
            this.client = client;
        }

        public async Task<List<Team>> GetTeams()
        {
            List<Team>? teams = await client.GetTeams();

            if (teams == null || teams.Count == 0)
            {
                throw new NhlException("No teams found", HttpStatusCode.NoContent);
            }

            return teams;
        }

        public async Task<Player> GetPlayer(int id)
        {
            Player? player = await client.GetPlayer(id);

            if (player == null)
            {
                throw new NhlException("No player found with id: " + id, HttpStatusCode.NotFound);
            }

            return player;
        }

        public async Task<List<Game>> GetGames(string date)
        {
            List<String> gamePaths = await client.GetScheduleGamesByDate(date);
            List<Game> games = new List<Game>();

            foreach (string path in gamePaths)
            {
                LiveFeed? liveFeed = await client.GetLiveFeed(path);

                if (liveFeed == null)
                {
                    throw new NhlException(
                        "No live feed found for path: " + path,
                        HttpStatusCode.NotFound
                    );
                }

                games.Add(await this.ConstructGame(liveFeed));
            }

            return games;
        }

        private async Task<Game> ConstructGame(LiveFeed feed)
        {
            List<Team> teams = await this.GetTeams();

            // TODO: actual game constructing

            return new Game();
        }
    }
}
