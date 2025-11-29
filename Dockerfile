### Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# copy project files and restore
COPY nhl-service-dotnet.sln .
COPY Program/nhl-service-dotnet.csproj Program/
COPY Tests/Tests.csproj Tests/
RUN dotnet restore Program/nhl-service-dotnet.csproj

# copy the rest of the source
COPY . .

# publish
RUN dotnet publish Program/nhl-service-dotnet.csproj -c Release -o /app/publish

### Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "nhl-service-dotnet.dll"]
