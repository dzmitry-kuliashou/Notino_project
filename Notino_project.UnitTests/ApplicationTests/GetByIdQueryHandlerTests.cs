using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Notino_project.Application.Caching;
using Notino_project.Application.Queries;
using Notino_project.Contract.Responses;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;
using Xunit;

namespace Notino_project.UnitTests.ApplicationTests
{
    public class GetByIdQueryHandlerTests
    {
        [Fact]
        public async Task GetById_DocumentDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var repository = new Mock<IDocumentsReadRepository>();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var handler = new GetByIdQueryHandler(repository.Object, cache);

            var result = await handler.Handle(new GetByIdQuery(id), CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
            repository.Verify(mock => mock.GetById(id), Times.Once);
            Assert.False(cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        [Fact]
        public async Task GetById_CacheMiss_ReturnsResponseAndCachesDocument()
        {
            var id = Guid.NewGuid();
            var document = CreateDocument(id);
            var repository = new Mock<IDocumentsReadRepository>();
            repository.Setup(mock => mock.GetById(id)).ReturnsAsync(document);

            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.SetupProperty(entry => entry.Value);
            cacheEntry.SetupProperty(entry => entry.AbsoluteExpirationRelativeToNow);
            cacheEntry.SetupProperty(entry => entry.SlidingExpiration);
            var cache = new Mock<IMemoryCache>();
            object? cachedValue = null;
            cache.Setup(mock => mock.TryGetValue(DocumentCacheKeys.For(id), out cachedValue)).Returns(false);
            cache.Setup(mock => mock.CreateEntry(DocumentCacheKeys.For(id))).Returns(cacheEntry.Object);

            var handler = new GetByIdQueryHandler(repository.Object, cache.Object);

            var result = await handler.Handle(new GetByIdQuery(id), CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentResponse = Assert.IsType<DocumentResponse>(response.Value);
            Assert.Equal(id, documentResponse.Id);
            repository.Verify(mock => mock.GetById(id), Times.Once);
            cache.Verify(mock => mock.CreateEntry(DocumentCacheKeys.For(id)), Times.Once);
            Assert.Same(document, cacheEntry.Object.Value);
            Assert.Equal(TimeSpan.FromMinutes(5), DocumentCachePolicy.AbsoluteExpirationRelativeToNow);
            Assert.Equal(TimeSpan.FromMinutes(2), DocumentCachePolicy.SlidingExpiration);
            Assert.Equal(DocumentCachePolicy.AbsoluteExpirationRelativeToNow, cacheEntry.Object.AbsoluteExpirationRelativeToNow);
            Assert.Equal(DocumentCachePolicy.SlidingExpiration, cacheEntry.Object.SlidingExpiration);
        }

        [Fact]
        public async Task GetById_CacheHit_DoesNotCallRepository()
        {
            var id = Guid.NewGuid();
            var document = CreateDocument(id);
            var repository = new Mock<IDocumentsReadRepository>();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(DocumentCacheKeys.For(id), document, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DocumentCachePolicy.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = DocumentCachePolicy.SlidingExpiration
            });
            var handler = new GetByIdQueryHandler(repository.Object, cache);

            var result = await handler.Handle(new GetByIdQuery(id), CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentResponse = Assert.IsType<DocumentResponse>(response.Value);
            Assert.Equal(id, documentResponse.Id);
            repository.Verify(mock => mock.GetById(It.IsAny<Guid>()), Times.Never);
        }

        private static Document CreateDocument(Guid id)
        {
            var document = Document.Construct(
                ["test"],
                new Dictionary<string, string> { ["name"] = "test" });
            document.Id = id;
            return document;
        }
    }
}
