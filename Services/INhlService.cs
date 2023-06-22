using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nhl_service_dotnet.Services
{
    public interface INhlService
    {
        Task<Team[]> GetTeams();
    }
}
