using System.Net;
using Microsoft.Extensions.Caching.Memory;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Services
{
    public class NhlService : INhlService
    {
        private readonly INhlClient client;
        private readonly IMemoryCache memoryCache;

        // TODO: move and use from environment variables
        public static readonly int CACHE_TIME = 5000; // millis

        public NhlService(INhlClient client, IMemoryCache memoryCache)
        {
            this.client = client;
            this.memoryCache = memoryCache;
        }

        public async Task<List<Team>> GetTeams()
        {
            List<Team>? teams = memoryCache.Get<List<Team>>(CacheKeys.Teams);

            if (teams is not null)
                return teams;

            teams = await client.GetTeams();

            if (teams == null || teams.Count == 0)
            {
                throw new NhlException("No teams found", HttpStatusCode.NoContent);
            }

            memoryCache.Set(CacheKeys.Teams, teams, TimeSpan.FromMilliseconds(CACHE_TIME));

            return teams;
        }

        public async Task<Player> GetPlayer(int id)
        {
            Player? player = memoryCache.Get<Player>(CacheKeys.Player);

            if (player is not null)
                return player;

            player = await client.GetPlayer(id);

            if (player == null)
            {
                throw new NhlException("No player found with id: " + id, HttpStatusCode.NotFound);
            }

            memoryCache.Set(CacheKeys.Player, player, TimeSpan.FromMilliseconds(CACHE_TIME));

            return player;
        }

        public async Task<List<Game>> GetGames(string date)
        {
            List<Game>? games = memoryCache.Get<List<Game>>(CacheKeys.Games);

            if (games is not null)
                return games;

            List<String> gamePaths = await client.GetScheduleGamesByDate(date);
            games = new List<Game>();

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

    public enum CacheKeys
    {
        Teams,
        Player,
        PlayerStats,
        Games
    }
}
