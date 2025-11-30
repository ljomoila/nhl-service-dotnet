# NHL Service (.NET)

Backend service that adapts the free NHL API to a client-friendly shape (used by the [235 NHL React Native app](https://github.com/ljomoila/235)). It also persists team/roster data and provides live game stats.

## Prerequisites
- .NET 7 SDK
- Docker (for container-based workflows)
- PostgreSQL (local or Cloud SQL)

## Running Locally
```bash
cd Program
dotnet run
```
Swagger UI: `http://localhost:5069/swagger/index.html`  
Routes: e.g., `http://localhost:5069/teams`, `http://localhost:5069/games/{date}`, `http://localhost:5069/teams/rosters`.

### With Docker Compose
```bash
docker-compose up --build
```
API: `http://localhost:8080` (connects to the bundled Postgres).

## Database
- EF Core with PostgreSQL. Tables: `teams`, `players`.
- Connection string is read from `ConnectionStrings__DefaultConnection` (env var overrides appsettings).
- On startup, migrations run automatically (`Database.Migrate()`).

## Tests
```bash
dotnet test Tests/Tests.csproj
```

## Deploying to Cloud Run (Artifact Registry + Cloud SQL)
1) Build/push image:
```bash
docker build -t us-central1-docker.pkg.dev/<PROJECT>/<REPO>/<SERVICE>:latest .
docker push us-central1-docker.pkg.dev/<PROJECT>/<REPO>/<SERVICE>:latest
```
2) Deploy:
```bash
gcloud run deploy <SERVICE> \
  --image us-central1-docker.pkg.dev/<PROJECT>/<REPO>/<SERVICE>:latest \
  --region us-central1 \
  --platform managed \
  --allow-unauthenticated \
  --add-cloudsql-instances <PROJECT>:us-central1:<INSTANCE> \
  --set-env-vars="ConnectionStrings__DefaultConnection=Host=/cloudsql/<PROJECT>:us-central1:<INSTANCE>;Database=postgres;Username=nhl;Password=<secret>,AppSettings__ApiKeyValue=<api-key>"
```

## API Key
Set `AppSettings__ApiKeyValue` (and `AppSettings__ApiKeyName` if you change the header) as an environment variable/secret. Swagger UI allows you to supply the API key header.
