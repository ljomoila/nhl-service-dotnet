using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Services
{
    public interface IGameService
    {
        Task<List<Game>> GetGames(string date);
    }
}
