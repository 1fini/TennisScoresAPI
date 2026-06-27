# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and restore
COPY *.sln .
COPY TennisScores.API/*.csproj ./TennisScores.API/
COPY TennisScores.Infrastructure/*.csproj ./TennisScores.Infrastructure/
COPY TennisScores.Domain/*.csproj ./TennisScores.Domain/
COPY TennisScores.Tests/*.csproj ./TennisScores.Tests/
RUN dotnet restore

# Copy remaining files and build
COPY . .
WORKDIR /src/TennisScores.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TennisScores.API.dll"]
