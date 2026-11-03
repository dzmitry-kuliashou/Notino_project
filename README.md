# Notino_project

Test task for Notino.

The solution is a .NET 8 document API. The API controller dispatches document operations to Application handlers through MediatR; handlers validate requests, work with the document aggregate and repository, manage caching, map responses, and return the API results.

## Projects

- **Notino_project** — ASP.NET Core API. `DocumentController` exposes `GET /documents/{id}`, `POST /documents`, and `PUT /documents/{id}` and forwards requests through `ISender`. `Program.cs` registers the Application handlers and validators, memory cache, and separate in-memory read and write repositories. Responses support JSON, XML, and MessagePack; JSON is used when the requested `Accept` format is unsupported.
- **Notino_project.Application** — MediatR query and command handlers, FluentValidation request validators, response mapper, validation messages, and shared cache key and expiration policy.
- **Notino_project.Contract** — API request and response models. `DocumentResponse.Data` is a string dictionary, while request data uses `JsonElement`.
- **Notino_project.Core** — `Document` aggregate, shared `AggregateRoot`, and the `IDocumentsReadRepository` / `IDocumentsWriteRepository` contracts. The aggregate enforces its tag and JSON-data invariants.
- **Notino_project.Infrastructure.FakeInMemoryStorage** — Separate fake read and write repository implementations backed by shared in-memory storage and a storage-specific model.
- **Notino_project.UnitTests** — Controller dispatch, Application handler and cache behavior, and request-validator unit tests.
- **Notino_project.IntegrationTests** — API formatter and validation tests that run through the real Application handlers with a repository test double.

## Application behavior

The get-by-id handler returns NotFound for missing documents and caches successful reads. Create and update handlers validate requests before repository operations and preserve the API response shapes. All handlers use the `documents_<id>` cache key with five-minute absolute and two-minute sliding expiration.

The validators and response mapper live in Application. Validation messages are shared constants so unit and integration tests verify the same API-visible text.

## Build and test

```powershell
dotnet build Notino_project.sln
dotnet test Notino_project.sln
```

## Docker

The solution includes a Dockerfile for running the application in a container.
