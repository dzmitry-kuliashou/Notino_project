# Notino_project

Test task for Notino.

The solution is a .NET 8 document API. The controller exposes `GET /documents`, `GET /documents/{id}`, `POST /documents`, `PUT /documents/{id}`, and `DELETE /documents/{id}` and dispatches operations to Application handlers through MediatR. Handlers validate requests, work with the document aggregate and repository, manage caching, map responses, and return API results.

## Projects

- **Notino_project** — ASP.NET Core API. `DocumentController` exposes the paginated `GET /documents` collection route, `GET /documents/{id}`, `POST /documents`, `PUT /documents/{id}`, and `DELETE /documents/{id}` and forwards requests through `ISender`. `Program.cs` registers the Application handlers and validators, memory cache, and MongoDB-backed read and write repositories. Responses support JSON, XML, and MessagePack; JSON is used when the requested `Accept` format is unsupported.
- **Notino_project.Application** — MediatR query and command handlers, FluentValidation request validators, response mapper, validation messages, and shared cache key and expiration policy.
- **Notino_project.Contract** — API request and response models. `DocumentResponse.Data` is a string dictionary, while request data uses `JsonElement`.
- **Notino_project.Core** — `Document` aggregate, shared `AggregateRoot`, and the `IDocumentsReadRepository` / `IDocumentsWriteRepository` contracts. The aggregate enforces its tag and JSON-data invariants.
- **Notino_project.Infrastructure** — MongoDB settings, BSON document mapping, and read/write repository implementations.
- **Notino_project.UnitTests** — Controller dispatch, Application handler and cache behavior, and request-validator unit tests.
- **Notino_project.IntegrationTests** — API formatter and validation tests with repository test doubles, plus repository tests against a real MongoDB instance.

## Application behavior

The get-by-id handler returns NotFound for missing documents and caches successful reads. The list handler returns a paginated document response and does not cache list results. Create and update handlers validate requests before repository operations and preserve the API response shapes. The get-by-id handler uses the `documents_<id>` cache key with five-minute absolute and two-minute sliding expiration.

The validators and response mapper live in Application. Validation messages are shared constants so unit and integration tests verify the same API-visible text.

## List documents

`GET /documents` returns a `DocumentsResponse` containing `items`, `totalPageCount`, `currentPage`, `pageSize`, `hasPrevious`, and `hasNext`.

Use the optional query parameters `p` (zero-based page index, default `0`) and `ps` (page size, default `10`). With the Compose API running, for example:

```powershell
Invoke-RestMethod -Uri "http://localhost:7000/documents?p=0&ps=10"
```

Malformed or negative `p` values use page `0`; malformed or non-positive `ps` values use page size `10`. If the page offset would overflow, the page index is clamped to the largest safe index for the selected page size. Results are ordered by document ID ascending. The repository uses an exact document count to calculate `totalPageCount`; the response does not expose a total item count. A valid page beyond the last page returns HTTP 200 with an empty `items` array and the requested page metadata.

## Build and test

```powershell
dotnet build Notino_project.sln
# Docker must be running; repository tests start their own MongoDB container.
dotnet test Notino_project.sln
```

## MongoDB local development

MongoDB configuration is read from the `MongoDb` section. `appsettings.json` provides non-secret local defaults (`mongodb://localhost:27017`, database `notino`, collection `documents`); normal .NET configuration environment variables such as `MongoDb__ConnectionString` override them.

Start the API and MongoDB with Compose. The API service is in the `notino_project` profile, so enable that profile explicitly:

```powershell
docker compose --profile notino_project up --build -d
docker compose ps
```

Compose waits for MongoDB's ping healthcheck before starting the API, persists database files in the `mongodb-data` named volume, publishes MongoDB on port `27017`, and serves the API at `http://localhost:7000` (host port `7000` mapped to container port `80`). The API's connection string defaults to `mongodb://mongo:27017` inside the Compose network. Override Compose settings with `MONGODB_API_CONNECTION_STRING`, `MONGODB_DATABASE_NAME`, and `MONGODB_COLLECTION_NAME`.

The repository tests use Testcontainers to start an isolated `mongo:8.0.4` container and dispose it after the test class finishes. Docker must be available to the test process; no external MongoDB service or connection-string environment variable is needed. Run only these tests with:

```powershell
dotnet test .\Notino_project.IntegrationTests\Notino_project.IntegrationTests.csproj
```

To smoke-test the API and persistence, create a document, then read and update it:

```powershell
$created = Invoke-RestMethod -Uri "http://localhost:7000/documents" -Method Post -ContentType "application/json" -Body '{"tags":["smoke"],"data":{"name":"before"}}'
$id = $created.id
Invoke-RestMethod -Uri "http://localhost:7000/documents/$id"
Invoke-RestMethod -Uri "http://localhost:7000/documents/$id" -Method Put -ContentType "application/json" -Body '{"tags":["smoke","updated"],"data":{"name":"after"}}'
```

Restart only the API with `docker compose restart notino_project`, wait for it to listen again, and repeat the GET. The updated document should still be available because MongoDB retains the data independently of the API container.

`docker compose down` preserves the named data volume; `docker compose down --volumes` removes it and deletes the local MongoDB data.
