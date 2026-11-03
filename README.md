# Notino_project
Test task for Notino

The Notino_project solution contains the following projects and components:

## 1. Notino_project
- `DocumentsController.cs`: This contains the `GetById` (GET), `Create` (POST), and `Update` (PUT) endpoints, which allow for the creation, updating, and retrieval of documents.
- `Notino_project.Contract`: Defines the API-facing `CreateDocumentRequest`, `UpdateDocumentRequest`, and `DocumentResponse` POCOs. Requests contain `Tags` and `JsonElement`-valued `Data`; the response contains `Id`, `Tags`, and dictionary-valued `Data`.
- `Request validation`: `CreateDocumentRequestValidator` and `UpdateDocumentRequestValidator` in the API project enforce nonempty `Tags` and a nonempty JSON object whose values are strings. The controller invokes these FluentValidation validators explicitly and returns all validation messages as a string array in a `400 Bad Request` response.
- `Response model`: `DocumentResponse.Data` is a `Dictionary<string, string>`, allowing it to be serialized into JSON, XML, and MessagePack. The API mapper converts the Core aggregate's `JsonElement` data into this dictionary.
- `API Response Format`: The API allows defining the return data format via the `Accept` request header. It supports JSON (by default), XML, and MessagePack formats. If any other `Accept` header is defined, the API returns data in JSON. New formats can be easily added using formatters in the `Program.cs` file.
- Mappers: The controller constructs or updates the Core aggregate from validated Contract request values, and the API mapper converts it to `DocumentResponse`.
## 2. Notino_project.Core
- This project contains the `Document` aggregate and shared `AggregateRoot`. The aggregate owns its identity and enforces nonempty tags and nonempty JSON objects containing only string values.
## 3. Notino_project.Services.Interfaces
- This project contains service interfaces (specifically, `IDocumentsService`), implementing the Dependency Injection principle. Although `IDocumentsService` includes all methods for manipulating documents, which breaks the Single Responsibility principle, it was done for simplicity in this small project. In a larger project, separate services for each operation would be preferable.
## 4. Notino_project.Services
- This project contains implementations of the service interfaces (specifically, `DocumentService`). `DocumentService` uses caching to improve the performance of the high-load Web API.
## 5. Notino_project.Repositories.Interfaces
- This project contains repository interfaces (specifically, `IDocumentsRepository`), defining methods for manipulating the data storage. This allows for the implementation of the Dependency Injection principle and the use of different real storages.
## 6. Notino_project.FakeInMemoryRepo
- This project contains a simple in-memory storage implementation, storing documents in a `ConcurrentDictionary<string, DocumentDal>`. A separate `DocumentDal` model is used for storing documents. Although it may not make much sense in this particular application (as we could store the Document model directly), real storages typically use their own data models.
## 7. Notino_project.UnitTests
- This project contains several XUnit unit tests. While the test coverage is not 100%, different kinds of tests are implemented to cover various areas of the application using various techniques:
  - ControllerTests: These tests check the business logic of controller methods by mocking `DocumentsService` and verifying that controller methods return the expected results in different situations.
  - ModelValidatorTests: These tests verify the FluentValidation rules for create and update requests.
  - ServicesTests: These tests check the business logic of the service, specifically the caching logic in `DocumentsService`.
## 8. Notino_project.IntegrationTests
- This project contains integration tests that mock `DocumentsService`, verify formatter behavior for different `Accept` headers, and verify invalid POST/PUT validation responses.

## Docker
Additionally, this solution includes a Dockerfile, allowing the creation of a Docker image to run the application as a Docker container.
