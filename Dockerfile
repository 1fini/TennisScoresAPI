# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY *.sln .
COPY TennisScores.API/*.csproj ./TennisScores.API/
COPY TennisScores.Infrastructure/*.csproj ./TennisScores.Infrastructure/
COPY TennisScores.Domain/*.csproj ./TennisScores.Domain/
RUN dotnet restore

# Copy remaining files and build
COPY . .
WORKDIR /src/TennisScores.API
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TennisScores.API.dll"]
