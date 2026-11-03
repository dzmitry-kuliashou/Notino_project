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
        private readonly IMongoCollection<MongoDocument> _collection;
        private readonly DocumentsReadRepository _readRepository;
        private readonly DocumentsWriteRepository _writeRepository;

        public DocumentsRepositoryTests(MongoDatabaseFixture fixture)
        {
            _collection = fixture.Database.GetCollection<MongoDocument>(
                $"documents_{Guid.NewGuid():N}");
            _readRepository = new DocumentsReadRepository(_collection);
            _writeRepository = new DocumentsWriteRepository(_collection);
        }

        [Fact]
        public async Task List_ReturnsAscendingPageAndExactCount()
        {
            var ids = Enumerable.Range(1, 5)
                .Select(index => Guid.Parse($"00000000-0000-0000-0000-{index:000000000000}"))
                .ToArray();
            using var jsonDocument = JsonDocument.Parse("""{"name":"paging"}""");

            foreach (var id in ids.Reverse())
            {
                var document = Document.Construct(["paging"], jsonDocument.RootElement.Clone());
                document.Id = id;
                await _writeRepository.Create(document);
            }

            var (documents, totalCount) = await _readRepository.List(1, 2);

            Assert.AreEqual(5L, totalCount);
            CollectionAssert.AreEqual(
                ids.Skip(2).Take(2).ToArray(),
                documents.Select(document => document.Id).ToArray());
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
        public async Task GetByIdAndList_LegacyDocumentWithoutDeletionFlag_TreatsItAsActive()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"legacy"}""");
            var document = Document.Construct(["legacy"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);
            await _collection.UpdateOneAsync(
                storedDocument => storedDocument.Id == document.Id,
                Builders<MongoDocument>.Update.Unset(storedDocument => storedDocument.IsDeleted));

            var result = await _readRepository.GetById(document.Id);
            var (documents, totalCount) = await _readRepository.List(0, 10);

            Assert.IsNotNull(result);
            Assert.AreEqual(document.Id, result.Id);
            Assert.AreEqual(1L, totalCount);
            CollectionAssert.AreEqual(new[] { document.Id }, documents.Select(item => item.Id).ToArray());
        }

        [Fact]
        public async Task GetById_DeletedDocument_ReturnsNull()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"deleted"}""");
            var document = Document.Construct(["deleted"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await _writeRepository.Delete(document);

            Assert.IsNull(await _readRepository.GetById(document.Id));
        }

        [Fact]
        public async Task List_DeletedDocument_ExcludesItFromPageAndCount()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"list"}""");
            var activeIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            foreach (var id in activeIds.Append(Guid.NewGuid()))
            {
                var document = Document.Construct(["list"], jsonDocument.RootElement.Clone());
                document.Id = id;
                await _writeRepository.Create(document);

                if (!activeIds.Contains(id))
                {
                    await _writeRepository.Delete(document);
                }
            }

            var (documents, totalCount) = await _readRepository.List(0, 10);

            Assert.AreEqual(2L, totalCount);
            CollectionAssert.AreEqual(
                activeIds.OrderBy(id => id).ToArray(),
                documents.Select(document => document.Id).ToArray());
        }

        [Fact]
        public async Task Update_DeletedDocument_ThrowsAggregateNotFoundException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"deleted"}""");
            var document = Document.Construct(["deleted"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await _writeRepository.Delete(document);

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Update(document));
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
        public async Task Create_DuplicateId_ThrowsAggregateAlreadyExistsException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"duplicate"}""");
            var document = Document.Construct(["duplicate"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await Assert.ThrowsExceptionAsync<AggregateAlreadyExistsException>(
                () => _writeRepository.Create(document));
        }

        [Fact]
        public async Task Update_MissingDocument_ThrowsAggregateNotFoundException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"missing"}""");
            var document = Document.Construct(["missing"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Update(document));

            Assert.IsNull(await _readRepository.GetById(document.Id));
        }

        [Fact]
        public async Task Delete_MissingDocument_ThrowsAggregateNotFoundException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"missing"}""");
            var document = Document.Construct(["missing"], jsonDocument.RootElement.Clone());
            document.Id = Guid.NewGuid();

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Delete(document));
        }
    }
}
