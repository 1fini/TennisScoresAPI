# Contributing to TennisScoresAPI

Thanks for your interest in contributing. TennisScoresAPI is the backend for an open-source live tennis scoring platform aimed at clubs, academies, associations, and amateur tournaments.

## Before You Start

- Check existing issues and pull requests to avoid duplicate work.
- For larger changes, open an issue first so the design can be discussed.
- Keep pull requests focused and reasonably small.

## Local Setup

Requirements:

- .NET 10 SDK
- Docker, if you want to run PostgreSQL or build images locally
- PostgreSQL 15+ for integration scenarios

Common commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project TennisScores.API/TennisScores.API.csproj
```

Swagger is available at `/swagger` when the API is running.

## Tests

Please add or update tests when changing scoring rules, API behavior, DTOs, persistence, or SignalR notifications.

Run the full test suite before opening a pull request:

```bash
dotnet test
```

## Pull Request Guidelines

- Explain the problem and the solution clearly.
- Include screenshots or API examples when helpful.
- Keep public DTO changes intentional and documented.
- Update the WebApp client contract when API changes require it.
- Do not commit secrets, production passwords, local `.env` files, or generated personal data.

## Useful Contribution Areas

- Tennis scoring edge cases: tie-breaks, super tie-breaks, completed matches, server rotation.
- Scoring engine extraction and domain tests.
- Health checks and production diagnostics.
- OpenAPI documentation.
- Developer onboarding and documentation.
