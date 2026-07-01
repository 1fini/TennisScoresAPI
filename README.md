# TennisScoresAPI

Backend API for live tennis scoring, tournament management, player management, and match lifecycle tracking.

TennisScoresAPI powers the TennisScore MVP. It exposes REST endpoints for tournaments, players, matches, and live scoring, persists match state in PostgreSQL with Entity Framework Core, and broadcasts live match updates through SignalR.

## Features

- Create, list, retrieve, and delete tournaments.
- Create, list, retrieve, and delete matches.
- Create, search, list, and delete players.
- Add points to live matches.
- Tennis scoring rules for standard games, tie-breaks, and final set super tie-breaks.
- Completed match protection: no point can be added after match completion.
- Winner and final score exposed in DTOs.
- SignalR hub for live score updates.
- OpenAPI/Swagger contract used by the Blazor WebApp client.
- Docker multi-stage build.
- GitHub Actions build, test, and multi-architecture Docker publication.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- Npgsql PostgreSQL provider
- PostgreSQL
- SignalR
- Swagger/OpenAPI
- Docker

## Repository Layout

```text
.
├── Dockerfile
├── TennisScores.sln
├── TennisScores.API/
│   ├── Controllers/
│   ├── Hubs/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
├── TennisScores.Domain/
│   ├── Dtos/
│   ├── Entities/
│   ├── Enums/
│   └── Repositories/
├── TennisScores.Infrastructure/
│   ├── Data/
│   ├── Migrations/
│   └── Repositories/
└── TennisScores.Tests/
```

## Requirements

- .NET 10 SDK
- PostgreSQL 15+ for integration scenarios
- Docker, if building or running the container image

## Configuration

The API reads database configuration from environment variables:

| Variable | Description | Example |
| --- | --- | --- |
| `DB_HOST` | PostgreSQL host | `localhost` |
| `DB_PORT` | PostgreSQL port | `5432` |
| `DB_NAME` | Database name | `tennisscore` |
| `DB_USER` | Database user | `tennisscore` |
| `DB_PASSWORD` | Database password | `change-me` |

In Docker production deployments, the WebApp compose stack sets these variables for the API container.

## Local Development

Restore and build:

```bash
dotnet restore
dotnet build
```

Run tests:

```bash
dotnet test
```

Run the API:

```bash
dotnet run --project TennisScores.API/TennisScores.API.csproj
```

Swagger UI is available when the API is running:

```text
/swagger
```

## Database Migrations

Entity Framework migrations live in:

```text
TennisScores.Infrastructure/Migrations
```

Apply migrations with:

```bash
dotnet ef database update \
  --project TennisScores.Infrastructure \
  --startup-project TennisScores.API
```

Make sure the `DB_*` environment variables are set before running migrations.

## API Surface

Primary endpoints:

| Area | Endpoint |
| --- | --- |
| Matches | `GET /api/Matches` |
| Match detail | `GET /api/Matches/{id}` |
| Create match | `POST /api/Matches` |
| Delete match | `DELETE /api/Matches/{id}` |
| Add point | `POST /api/LiveScore/add-point` |
| Tournaments | `GET /api/Tournaments` |
| Tournament detail | `GET /api/Tournaments/{id}` |
| Create tournament | `POST /api/Tournaments` |
| Delete tournament | `DELETE /api/Tournaments/{id}` |

The SignalR hub is exposed at:

```text
/scoreHub
```

## Docker

Build the API image:

```bash
docker build -t 1fini/tennisscoreapi:local .
```

The Dockerfile uses:

- .NET SDK 10 for build
- ASP.NET Core runtime 10 for execution
- `Release` publish
- HTTP port `8080`

The API image is intended to run behind the WebApp production compose stack, where it is reachable internally as:

```text
http://api:8080/
```

### Migration Image

The repository also provides `Dockerfile.migrations`, which builds an EF Core migration bundle image:

```bash
docker build -f Dockerfile.migrations -t 1fini/tennisscoreapi-migrations:local .
```

The migration image does not require the .NET SDK or source code on the production server. It runs the compiled EF migration bundle against the database configured with `DB_*` environment variables.

## GitHub Actions

The main workflow:

```text
.github/workflows/buildv2.yaml
```

It performs:

- restore
- build
- tests
- Docker image build
- DockerHub publication on push
- multi-architecture image publication for `linux/amd64` and `linux/arm64`
- migration image publication as `1fini/tennisscoreapi-migrations`

## Production Deployment

The recommended MVP deployment is managed from the WebApp repository with:

```text
docker-compose.prod.yml
```

That compose stack runs:

- Traefik
- TennisScoreWebApp
- TennisScoresAPI
- PostgreSQL
- optional one-shot database migrations

Only the WebApp is exposed publicly. The API remains internal to Docker networking.

## Roadmap

- Extract a pure scoring engine from the application service layer.
- Add health check endpoints.
- Harden Swagger exposure for production.
- Add authentication for MVP users.
- Improve production observability and structured logging.

## Related Repository

Frontend WebApp:

- https://github.com/1fini/TennisScoreWebApp

## License

This project is licensed under the terms of the repository license.
