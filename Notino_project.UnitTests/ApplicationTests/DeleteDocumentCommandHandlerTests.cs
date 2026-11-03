using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Notino_project.Application.Caching;
using Notino_project.Application.Commands;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ApplicationTests
{
    public class DeleteDocumentCommandHandlerTests : IDisposable
    {
        private readonly Mock<IDocumentsReadRepository> _readRepository = new();
        private readonly Mock<IDocumentsWriteRepository> _writeRepository = new();
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly DeleteDocumentCommandHandler _handler;

        public DeleteDocumentCommandHandlerTests()
        {
            _handler = new DeleteDocumentCommandHandler(
                _readRepository.Object,
                _writeRepository.Object,
                _cache);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        [Fact]
        public async Task Delete_DocumentDoesNotExist_ReturnsNotFoundAndEvictsCache()
        {
            var id = Guid.NewGuid();
            _readRepository
                .Setup(mock => mock.GetById(id))
                .ReturnsAsync((Document?)null);
            _cache.Set(DocumentCacheKeys.For(id), CreateDocument(id));

            var result = await _handler.Handle(
                new DeleteDocumentCommand(id),
                CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
            _readRepository.Verify(mock => mock.GetById(id), Times.Once);
            _writeRepository.Verify(mock => mock.Update(It.IsAny<Document>()), Times.Never);
            Assert.False(_cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        [Fact]
        public async Task Delete_ExistingDocument_ReturnsNoContentAndEvictsCache()
        {
            var id = Guid.NewGuid();
            var existing = CreateDocument(id);
            _readRepository.Setup(mock => mock.GetById(id)).ReturnsAsync(existing);
            _writeRepository
                .Setup(mock => mock.Update(It.IsAny<Document>()))
                .ReturnsAsync(existing);
            _cache.Set(DocumentCacheKeys.For(id), existing);

            var result = await _handler.Handle(
                new DeleteDocumentCommand(id),
                CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
            _readRepository.Verify(mock => mock.GetById(id), Times.Once);
            _writeRepository.Verify(
                mock => mock.Update(It.Is<Document>(document => document.Id == id && document.IsDeleted)),
                Times.Once);
            Assert.False(_cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        [Fact]
        public async Task Delete_RepositoryReportsMissingDocument_ReturnsNotFoundAndEvictsCache()
        {
            var id = Guid.NewGuid();
            var existing = CreateDocument(id);
            _readRepository.Setup(mock => mock.GetById(id)).ReturnsAsync(existing);
            _writeRepository
                .Setup(mock => mock.Update(It.IsAny<Document>()))
                .ThrowsAsync(new DocumentNotFoundException(id));
            _cache.Set(DocumentCacheKeys.For(id), existing);

            var result = await _handler.Handle(
                new DeleteDocumentCommand(id),
                CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
            _readRepository.Verify(mock => mock.GetById(id), Times.Once);
            _writeRepository.Verify(
                mock => mock.Update(It.Is<Document>(document => document.Id == id && document.IsDeleted)),
                Times.Once);
            Assert.False(_cache.TryGetValue(DocumentCacheKeys.For(id), out _));
        }

        private static Document CreateDocument(Guid id)
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":"test"}""");
            var document = Document.Construct(["test"], jsonDocument.RootElement.Clone());
            document.Id = id;
            return document;
        }
    }
}
