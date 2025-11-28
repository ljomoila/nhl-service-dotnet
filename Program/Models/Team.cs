using System.Text.Json.Serialization;

namespace nhl_service_dotnet;

public class Team
{
    public int id { get; set; }

    public string? name { get; set; }

    public string? shortName { get; set; }

    public string? abbreviation { get; set; }

    [JsonPropertyName("apiLink")]
    public string? link { get; set; }
}
