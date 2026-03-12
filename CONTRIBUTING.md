# Contributing to DEKE

Thanks for your interest in contributing! Here's how to get started.

## Local Setup

1. **Prerequisites**: .NET 9 SDK, PostgreSQL 16 with pgvector (local install or via container)
2. **Start the database** (if using a container): `podman-compose up -d`
3. **Download the embedding model**: `./scripts/download-model.sh` (~100 MB)
4. **Build**: `dotnet build`
5. **Run tests**: `dotnet test`
6. **Run the API**: `dotnet run --project src/Deke.Api`

## Coding Conventions

- File-scoped namespaces (`namespace Foo;`)
- Private fields prefixed with `_` (`private readonly Foo _foo;`)
- Records for DTOs and immutable models
- `CancellationToken` on all async methods
- 4-space indentation (see `.editorconfig`)
- New endpoints require auth by default (fallback policy). Add `.AllowAnonymous()` for read endpoints or `.RequireAuthorization()` for write endpoints explicitly.

## PR Process

1. Fork the repo and create a feature branch from `main`
2. Make your changes and ensure `dotnet build` and `dotnet test` pass
3. Write a clear PR description explaining what and why
4. CI will run build + tests automatically

## Areas That Could Use Help

- **Harvesters**: New source types (GitHub Issues, PDFs, YouTube transcripts)
- **Tests**: Unit and integration test coverage
- **Documentation**: Tutorials, example use cases
- **Performance**: Embedding generation caching, query optimization

## Questions?

Open an issue — happy to help!
