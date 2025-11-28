using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Integrations;
using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public class PlayerService : ServiceBase, IPlayerService
    {
        private readonly INhlClient client;
        private readonly IMemoryCache cache;

        public PlayerService(INhlClient client, IMemoryCache cache, IOptions<AppSettings>? settings = null) : base(settings)
        {
            this.client = client;
            this.cache = cache;
        }

        public async Task<Player> GetPlayer(int id)
        {
            string cacheKey = $"Player_{id}";
            if (cache.TryGetValue(cacheKey, out Player? cached) && cached != null)
            {
                return cached;
            }

            Player? player = await client.GetPlayer(id);

            if (player == null)
            {
                throw new NhlException("No player found with id: " + id, HttpStatusCode.NotFound);
            }

            cache.Set(cacheKey, player, TimeSpan.FromSeconds(this.settings.Cache.PlayersSeconds));
            return player;
        }
    }
}
