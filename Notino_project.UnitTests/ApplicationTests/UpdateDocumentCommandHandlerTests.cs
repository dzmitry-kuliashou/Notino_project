using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Notino_project.Application.Commands;
using Notino_project.Application.Caching;
using Notino_project.Application.Errors;
using Notino_project.Application.Validators;
using Notino_project.Contract.Requests;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;
using Xunit;

namespace Notino_project.UnitTests.ApplicationTests
{
    public class UpdateDocumentCommandHandlerTests
    {
        [Fact]
        public async Task Update_InvalidRequest_ReturnsBadRequestWithoutReading()
        {
            var id = Guid.NewGuid();
            var readRepository = new Mock<IDocumentsReadRepository>();
            var writeRepository = new Mock<IDocumentsWriteRepository>();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var handler = new UpdateDocumentCommandHandler(
                readRepository.Object,
                writeRepository.Object,
                new UpdateDocumentRequestValidator(),
                cache);
            var request = new UpdateDocumentRequest
            {
                Tags = Array.Empty<string>()
            };

            var result = await handler.Handle(
                new UpdateDocumentCommand(id, request),
                CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<string[]>(badRequest.Value);
            Assert.Equal(
                new[]
                {
                    DocumentValidationErrors.TagsCollectionShouldNotBeEmpty,
                    DocumentValidationErrors.DataMustHaveAtLeastOneProperty
                },
                errors);
            readRepository.Verify(mock => mock.GetById(It.IsAny<Guid>()), Times.Never);
            writeRepository.Verify(mock => mock.Update(It.IsAny<Document>()), Times.Never);
        }

        [Fact]
        public async Task Update_DocumentDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var readRepository = new Mock<IDocumentsReadRepository>();
            var writeRepository = new Mock<IDocumentsWriteRepository>();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var handler = new UpdateDocumentCommandHandler(
                readRepository.Object,
                writeRepository.Object,
                new UpdateDocumentRequestValidator(),
                cache);

            var result = await handler.Handle(
                new UpdateDocumentCommand(id, CreateValidRequest()),
                CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
            readRepository.Verify(mock => mock.GetById(id), Times.Once);
            writeRepository.Verify(mock => mock.Update(It.IsAny<Document>()), Times.Never);
            Assert.False(cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        [Fact]
        public async Task Update_RepositoryReportsMissingDocument_ReturnsNotFoundAndRemovesCachedDocument()
        {
            var id = Guid.NewGuid();
            var existing = Document.Construct(
                ["old"],
                new Dictionary<string, string> { ["name"] = "old" });
            existing.Id = id;
            var readRepository = new Mock<IDocumentsReadRepository>();
            readRepository.Setup(mock => mock.GetById(id)).ReturnsAsync(existing);
            var writeRepository = new Mock<IDocumentsWriteRepository>();
            writeRepository
                .Setup(mock => mock.Update(It.IsAny<Document>()))
                .ThrowsAsync(new AggregateNotFoundException(id));
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var handler = new UpdateDocumentCommandHandler(
                readRepository.Object,
                writeRepository.Object,
                new UpdateDocumentRequestValidator(),
                cache);

            var result = await handler.Handle(
                new UpdateDocumentCommand(id, CreateValidRequest()),
                CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
            readRepository.Verify(mock => mock.GetById(id), Times.Once);
            writeRepository.Verify(mock => mock.Update(It.Is<Document>(document => document.Id == id)), Times.Once);
            Assert.False(cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnsResponseAndCachesDocument()
        {
            var id = Guid.NewGuid();
            var existing = Document.Construct(
                ["old"],
                new Dictionary<string, string> { ["name"] = "old" });
            existing.Id = id;
            var request = CreateValidRequest();
            var readRepository = new Mock<IDocumentsReadRepository>();
            readRepository.Setup(mock => mock.GetById(id)).ReturnsAsync(existing);
            var writeRepository = new Mock<IDocumentsWriteRepository>();
            writeRepository.Setup(mock => mock.Update(It.IsAny<Document>())).ReturnsAsync(existing);

            var cacheEntries = new List<Mock<ICacheEntry>>();
            var cache = new Mock<IMemoryCache>();
            object? cachedValue = null;
            cache.Setup(mock => mock.TryGetValue(DocumentCacheKeys.For(id), out cachedValue)).Returns(false);
            cache.Setup(mock => mock.CreateEntry(DocumentCacheKeys.For(id))).Returns(() =>
            {
                var entry = new Mock<ICacheEntry>();
                entry.SetupProperty(cacheEntry => cacheEntry.Value);
                entry.SetupProperty(cacheEntry => cacheEntry.AbsoluteExpirationRelativeToNow);
                entry.SetupProperty(cacheEntry => cacheEntry.SlidingExpiration);
                cacheEntries.Add(entry);
                return entry.Object;
            });

            var handler = new UpdateDocumentCommandHandler(
                readRepository.Object,
                writeRepository.Object,
                new UpdateDocumentRequestValidator(),
                cache.Object);

            var result = await handler.Handle(
                new UpdateDocumentCommand(id, request),
                CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentResponse = Assert.IsType<Contract.Responses.DocumentResponse>(response.Value);
            Assert.Equal(id, documentResponse.Id);
            Assert.Equal(new[] { "test" }, documentResponse.Tags);
            Assert.Equal("test", documentResponse.Data["name"]);
            readRepository.Verify(mock => mock.GetById(id), Times.Once);
            writeRepository.Verify(mock => mock.Update(It.Is<Document>(document => document.Id == id)), Times.Once);
            cache.Verify(mock => mock.CreateEntry(DocumentCacheKeys.For(id)), Times.Exactly(2));
            Assert.Same(existing, cacheEntries[1].Object.Value);
            Assert.Equal(DocumentCachePolicy.AbsoluteExpirationRelativeToNow, cacheEntries[1].Object.AbsoluteExpirationRelativeToNow);
            Assert.Equal(DocumentCachePolicy.SlidingExpiration, cacheEntries[1].Object.SlidingExpiration);
        }

        private static UpdateDocumentRequest CreateValidRequest()
        {
            return new UpdateDocumentRequest
            {
                Tags = ["test"],
                Data = new Dictionary<string, string> { ["name"] = "test" }
            };
        }
    }
}
