# NHL service (dotnet)

Middleman service between free [NHL api](https://statsapi.web.nhl.com/api/v1/teams) and [235 NHL React Native app](https://github.com/ljomoila/235).

## Development

Make sure you have .NET version 7 or higher intalled.

### Run and Debug

-   `cd Program`
-   `dotnet run` - Runs the application and you can then test it in `http://localhost:5069/{route}`
-   `dotnet watch run` - Runs the application, watches changes and opens it up in `http://localhost:5087/swagger/index.html`

### Build

-   `dotnet build` - Builds project

### Test

-   `cd Tests`
-   `dotnet test` - Runs tests
