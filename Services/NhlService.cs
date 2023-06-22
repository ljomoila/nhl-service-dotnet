using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nhl_service_dotnet.Integrations;

namespace nhl_service_dotnet.Services
{
    public class NhlService : INhlService
    {
        private readonly NhlClient client;

        public NhlService(NhlClient client)
        {
            this.client = client;
        }

        public async Task<Team[]> GetTeams()
        {
            return await this.client.GetTeams();
        }
    }
}
