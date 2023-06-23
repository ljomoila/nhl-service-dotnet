using Swashbuckle.AspNetCore.Annotations;

namespace nhl_service_dotnet;

[Serializable]
public class Team
{
    public int id { get; set; }

    public string? name { get; set; }

    public string? shortName { get; set; }

    public string? abbreviation { get; set; }

    public string? apiLink { get; set; }
}
