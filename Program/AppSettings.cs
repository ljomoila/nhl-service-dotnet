namespace nhl_service_dotnet
{
    public class AppSettings
    {
        public string ApiBase { get; set; } = "https://api-web.nhle.com/v1";
        public CacheSettings Cache { get; set; } = new CacheSettings();
        public string ApiKeyName { get; set; } = "X-API-Key";
        public string ApiKeyValue { get; set; } = "changeme";
    }

    public class CacheSettings
    {
        public int TeamsSeconds { get; set; } = 86400; // 1 day
        public int PlayersSeconds { get; set; } = 86400; // 1 day
        public int GamesSeconds { get; set; } = 60; // 60 seconds
    }
}
