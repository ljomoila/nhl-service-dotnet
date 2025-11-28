using nhl_service_dotnet.Models;

namespace nhl_service_dotnet.Services
{
    public interface IPlayerService
    {
        Task<Player> GetPlayer(int id);
    }
}
