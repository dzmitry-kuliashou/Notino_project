using MongoDB.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Infrastructure.Repositories;
using System.Text.Json;
using Testcontainers.MongoDb;
using Xunit;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.IntegrationTests
{
    public sealed class MongoDatabaseFixture : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder("mongo:8.0.4").Build();
        private MongoClient? _client;
        private IMongoDatabase? _database;

        public IMongoDatabase Database =>
            _database ?? throw new InvalidOperationException("The MongoDB fixture has not been initialized.");

        public async Task InitializeAsync()
        {
            await _mongoDbContainer.StartAsync();

            _client = new MongoClient(_mongoDbContainer.GetConnectionString());
            _database = _client.GetDatabase($"notino_integration_{Guid.NewGuid():N}");
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            await _mongoDbContainer.DisposeAsync();
        }
    }

    public sealed class DocumentsRepositoryTests : IClassFixture<MongoDatabaseFixture>
    {
        private readonly DocumentsReadRepository _readRepository;
        private readonly DocumentsWriteRepository _writeRepository;

        public DocumentsRepositoryTests(MongoDatabaseFixture fixture)
        {
            var collection = fixture.Database.GetCollection<MongoDocument>("documents");
            _readRepository = new DocumentsReadRepository(collection);
            _writeRepository = new DocumentsWriteRepository(collection);
        }

        [Fact]
        public async Task Create_GetById_PreservesGuidTagsAndData()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"item","category":"care"}""");
            var document = Document.Construct(
                ["skincare", "daily"],
                jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();

            await _writeRepository.Create(document);
            var result = await _readRepository.GetById(document.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(document.Id, result.Id);
            CollectionAssert.AreEqual(new[] { "skincare", "daily" }, result.Tags.ToArray());
            Assert.AreEqual("item", result.Data.GetProperty("name").GetString());
            Assert.AreEqual("care", result.Data.GetProperty("category").GetString());
        }

        [Fact]
        public async Task Update_ExistingDocument_PersistsChangedFields()
        {
            using var initialJson = JsonDocument.Parse("""{"name":"old"}""");
            var document = Document.Construct(["old"], initialJson.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            using var updatedJson = JsonDocument.Parse("""{"name":"new","status":"active"}""");
            document.Update(["new", "updated"], updatedJson.RootElement.Clone());
            await _writeRepository.Update(document);

            var result = await _readRepository.GetById(document.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(document.Id, result.Id);
            CollectionAssert.AreEqual(new[] { "new", "updated" }, result.Tags.ToArray());
            Assert.AreEqual("new", result.Data.GetProperty("name").GetString());
            Assert.AreEqual("active", result.Data.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Create_DuplicateId_ThrowsDocumentAlreadyExistsException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"duplicate"}""");
            var document = Document.Construct(["duplicate"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await Assert.ThrowsExceptionAsync<DocumentAlreadyExistsException>(
                () => _writeRepository.Create(document));
        }

        [Fact]
        public async Task Update_MissingDocument_ThrowsDocumentNotFoundException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"missing"}""");
            var document = Document.Construct(["missing"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();

            await Assert.ThrowsExceptionAsync<DocumentNotFoundException>(
                () => _writeRepository.Update(document));

            Assert.IsNull(await _readRepository.GetById(document.Id));
        }
    }
}
