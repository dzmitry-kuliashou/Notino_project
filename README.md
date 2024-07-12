# Notino_project
Test task for Notino

The Notino_project solution contains the following projects and components:

## 1. Notino_project
- `DocumentsController.cs`: This contains the `GetById` (GET), `Create` (POST), and `Update` (PUT) endpoints, which allow for the creation, updating, and retrieval of documents.
- `DTO Model Classes`: These define the input and output models for creating, updating, and receiving documents via the API.
- `Create` and `Update` Models: These models have validation attributes for client input data validation:
  - `Id` is a mandatory `GUID` property.
  - `Tags` is a mandatory array that must contain at least one `string` member.
  - `Data` is a mandatory `JsonElement` property, allowing for an arbitrary schema. It must be an object type with at least one property, and only string properties are allowed. Validation rules are defined in the `SimplePropertiesOnlyAttribute` class.
- `Return Model`: The `Data` field is defined as `Dictionary<string, string>`, allowing it to be serialized into different data formats (JSON, XML, MessagePack). For deserializing `JsonElement` into `Dictionary<string, string>`, a custom `DynamicDataModelBinder` is used.
- `API Response Format`: The API allows defining the return data format via the `Accept` request header. It supports JSON (by default), XML, and MessagePack formats. If any other `Accept` header is defined, the API returns data in JSON. New formats can be easily added using formatters in the `Program.cs` file.
- Mappers: These convert DTO models to/from service layer models.
## 2. Notino_project.Models
- This project contains models (specifically, the `Document` model) for the application's service layer, allowing for a complete separation of client-distributed models from internal application models.
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
  - ModelValidatorTests: These tests ensure that model validators return the expected error messages for different invalid input model states.
  - ServicesTests: These tests check the business logic of the service, specifically the caching logic in `DocumentsService`.
## 8. Notino_project.IntegrationTests
- This project contains integration tests that mock `DocumentsService` and validate that `DocumentsController` returns data in the expected format for different values of the `Accept` request header.

## Docker
Additionally, this solution includes a Dockerfile, allowing the creation of a Docker image to run the application as a Docker container.

