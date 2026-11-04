using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Infrastructure.Repositories;
using Testcontainers.MongoDb;
using Xunit;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.IntegrationTests.RepositoryTests
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
            var data = new Dictionary<string, string> { ["name"] = "paging" };

            foreach (var id in ids.Reverse())
            {
                var document = Document.Construct(["paging"], data);
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
            var document = Document.Construct(
                ["skincare", "daily"],
                new Dictionary<string, string>
                {
                    ["name"] = "item",
                    ["category"] = "care",
                });
            document.Id = Guid.NewGuid();

            await _writeRepository.Create(document);
            var result = await _readRepository.GetById(document.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(document.Id, result.Id);
            CollectionAssert.AreEqual(new[] { "skincare", "daily" }, result.Tags.ToArray());
            Assert.AreEqual("item", result.Data["name"]);
            Assert.AreEqual("care", result.Data["category"]);
        }

        [Fact]
        public async Task GetByIdAndList_LegacyDocumentWithoutDeletionFlag_TreatsItAsActive()
        {
            var document = Document.Construct(
                ["legacy"],
                new Dictionary<string, string> { ["name"] = "legacy" });
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
            var document = Document.Construct(
                ["deleted"],
                new Dictionary<string, string> { ["name"] = "deleted" });
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await _writeRepository.Delete(document);

            Assert.IsNull(await _readRepository.GetById(document.Id));
        }

        [Fact]
        public async Task List_DeletedDocument_ExcludesItFromPageAndCount()
        {
            var data = new Dictionary<string, string> { ["name"] = "list" };
            var activeIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            foreach (var id in activeIds.Append(Guid.NewGuid()))
            {
                var document = Document.Construct(["list"], data);
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
            var document = Document.Construct(
                ["deleted"],
                new Dictionary<string, string> { ["name"] = "deleted" });
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await _writeRepository.Delete(document);

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Update(document));
        }

        [Fact]
        public async Task Update_ExistingDocument_PersistsChangedFields()
        {
            var document = Document.Construct(
                ["old"],
                new Dictionary<string, string> { ["name"] = "old" });
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            document.Update(
                ["new", "updated"],
                new Dictionary<string, string>
                {
                    ["name"] = "new",
                    ["status"] = "active",
                });
            await _writeRepository.Update(document);

            var result = await _readRepository.GetById(document.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(document.Id, result.Id);
            CollectionAssert.AreEqual(new[] { "new", "updated" }, result.Tags.ToArray());
            Assert.AreEqual("new", result.Data["name"]);
            Assert.AreEqual("active", result.Data["status"]);
        }

        [Fact]
        public async Task Create_DuplicateId_ThrowsAggregateAlreadyExistsException()
        {
            var document = Document.Construct(
                ["duplicate"],
                new Dictionary<string, string> { ["name"] = "duplicate" });
            document.Id = Guid.NewGuid();
            await _writeRepository.Create(document);

            await Assert.ThrowsExceptionAsync<AggregateAlreadyExistsException>(
                () => _writeRepository.Create(document));
        }

        [Fact]
        public async Task Update_MissingDocument_ThrowsAggregateNotFoundException()
        {
            var document = Document.Construct(
                ["missing"],
                new Dictionary<string, string> { ["name"] = "missing" });
            document.Id = Guid.NewGuid();

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Update(document));

            Assert.IsNull(await _readRepository.GetById(document.Id));
        }

        [Fact]
        public async Task Delete_MissingDocument_ThrowsAggregateNotFoundException()
        {
            var document = Document.Construct(
                ["missing"],
                new Dictionary<string, string> { ["name"] = "missing" });
            document.Id = Guid.NewGuid();

            await Assert.ThrowsExceptionAsync<AggregateNotFoundException>(
                () => _writeRepository.Delete(document));
        }
    }
}
