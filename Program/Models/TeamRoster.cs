using nhl_service_dotnet.Models;
using System.Collections.Generic;

namespace nhl_service_dotnet.Models
{
    public class TeamRoster
    {
        public Team? team { get; set; }
        public List<Player>? players { get; set; }
    }
}
