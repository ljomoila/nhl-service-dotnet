using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
