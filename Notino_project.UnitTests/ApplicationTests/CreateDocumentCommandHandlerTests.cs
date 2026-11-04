using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Notino_project.Application.Commands;
using Notino_project.Application.Caching;
using Notino_project.Application.Errors;
using Notino_project.Application.Validators;
using Notino_project.Contract.Requests;
using Notino_project.Contract.Responses;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;
using Xunit;

namespace Notino_project.UnitTests.ApplicationTests
{
    public class CreateDocumentCommandHandlerTests
    {
        [Fact]
        public async Task Create_InvalidRequest_ReturnsBadRequestWithoutCreating()
        {
            var repository = new Mock<IDocumentsWriteRepository>();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var handler = new CreateDocumentCommandHandler(
                repository.Object,
                new CreateDocumentRequestValidator(),
                cache);
            var request = new CreateDocumentRequest
            {
                Tags = Array.Empty<string>()
            };

            var result = await handler.Handle(new CreateDocumentCommand(request), CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<string[]>(badRequest.Value);
            Assert.Equal(
                new[]
                {
                    DocumentValidationErrors.TagsCollectionShouldNotBeEmpty,
                    DocumentValidationErrors.DataMustHaveAtLeastOneProperty
                },
                errors);
            repository.Verify(mock => mock.Create(It.IsAny<Document>()), Times.Never);
        }

        [Fact]
        public async Task Create_DuplicateDocument_ReturnsConflict()
        {
            var repository = new Mock<IDocumentsWriteRepository>();
            repository
                .Setup(mock => mock.Create(It.IsAny<Document>()))
                .ThrowsAsync(new AggregateAlreadyExistsException(Guid.NewGuid()));
            var cache = new Mock<IMemoryCache>();
            var handler = new CreateDocumentCommandHandler(
                repository.Object,
                new CreateDocumentRequestValidator(),
                cache.Object);

            var result = await handler.Handle(
                new CreateDocumentCommand(CreateValidRequest()),
                CancellationToken.None);

            Assert.IsType<ConflictResult>(result);
            repository.Verify(mock => mock.Create(It.IsAny<Document>()), Times.Once);
            cache.Verify(mock => mock.CreateEntry(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAndCachesDocument()
        {
            var id = Guid.NewGuid();
            var request = CreateValidRequest();
            var document = Document.Construct(request.Tags, request.Data);
            document.Id = id;
            var repository = new Mock<IDocumentsWriteRepository>();
            repository.Setup(mock => mock.Create(It.IsAny<Document>())).ReturnsAsync(document);

            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.SetupProperty(entry => entry.Value);
            cacheEntry.SetupProperty(entry => entry.AbsoluteExpirationRelativeToNow);
            cacheEntry.SetupProperty(entry => entry.SlidingExpiration);
            var cache = new Mock<IMemoryCache>();
            cache.Setup(mock => mock.CreateEntry(DocumentCacheKeys.For(id))).Returns(cacheEntry.Object);

            var handler = new CreateDocumentCommandHandler(
                repository.Object,
                new CreateDocumentRequestValidator(),
                cache.Object);

            var result = await handler.Handle(new CreateDocumentCommand(request), CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("Create", createdAt.ActionName);
            Assert.Equal(id, createdAt.RouteValues?["id"]);
            var response = Assert.IsType<DocumentResponse>(createdAt.Value);
            Assert.Equal(id, response.Id);
            repository.Verify(mock => mock.Create(It.IsAny<Document>()), Times.Once);
            cache.Verify(mock => mock.CreateEntry(DocumentCacheKeys.For(id)), Times.Once);
            Assert.Same(document, cacheEntry.Object.Value);
            Assert.Equal(DocumentCachePolicy.AbsoluteExpirationRelativeToNow, cacheEntry.Object.AbsoluteExpirationRelativeToNow);
            Assert.Equal(DocumentCachePolicy.SlidingExpiration, cacheEntry.Object.SlidingExpiration);
        }

        private static CreateDocumentRequest CreateValidRequest()
        {
            return new CreateDocumentRequest
            {
                Tags = ["test"],
                Data = new Dictionary<string, string> { ["name"] = "test" }
            };
        }
    }
}
