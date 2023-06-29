# NHL service (.NET)

Business service between free [NHL api](https://statsapi.web.nhl.com/api/v1/teams) and clients ([235 NHL React Native app](https://github.com/ljomoila/235)).
Purpose of the service is to map data from NHL api to cleaner form for the clients.

## Development

Make sure you have .NET version 7 or higher intalled.

### Run

-   `cd Program`
-   `dotnet run` - Runs the application and you can then test it in `http://localhost:5069/{route}`
-   `dotnet watch run` - Runs the application, watches changes and opens Swagger documentation in `http://localhost:5069/swagger/index.html`

### Build

-   `dotnet build` - Builds project

### Test

-   `cd Tests`
-   `dotnet test` - Runs tests
