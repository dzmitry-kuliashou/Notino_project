using Microsoft.Extensions.Caching.Memory;
using Moq;
using Notino_project.Models.Models;
using Notino_project.Repositories.Interfaces.Repositories;
using Notino_project.Services.Services.Documents;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ServicesTests
{
    public class DocumentsServiceTests
    {
        [Fact]
        public async Task GetById_DocumentIsNotInCache_ShouldCallDocumentsRepository()
        {
            var id = Guid.NewGuid();

            var document = GenerateValidDocument(id);

            var mockDocumentsRepository = new Mock<IDocumentsRepository>();
            mockDocumentsRepository.Setup(m => m.GetById(id)).ReturnsAsync(document);

            var cache = new MemoryCache(new MemoryCacheOptions());

            var documentsService = new DocumentsService(mockDocumentsRepository.Object, cache);

            var existing = await documentsService.GetById(id);

            mockDocumentsRepository.Verify(s => s.GetById(id), Times.Once);
        }

        [Fact]
        public async Task GetById_DocumentIsInCache_ShouldNotCallDocumentsRepository()
        {
            var id = Guid.NewGuid();

            var document = GenerateValidDocument(id);

            var mockDocumentsRepository = new Mock<IDocumentsRepository>();
            mockDocumentsRepository.Setup(m => m.GetById(id)).ReturnsAsync(document);

            var cache = new MemoryCache(new MemoryCacheOptions());

            var documentsService = new DocumentsService(mockDocumentsRepository.Object, cache);

            var existing = await documentsService.GetById(id);

            mockDocumentsRepository.Verify(s => s.GetById(id), Times.Once);

            existing = await documentsService.GetById(id);

            mockDocumentsRepository.Verify(s => s.GetById(id), Times.Once);
        }

        private Document GenerateValidDocument(Guid id)
        {
            var jsonInput = @"{
                        ""data"": 
                        {
                            ""something"": ""This is data""
                        }
                    }";

            JsonElement data;
            using (var doc = JsonDocument.Parse(jsonInput))
            {
                var root = doc.RootElement;
                data = root.GetProperty("data").Clone();
            }

            return new Document
            {
                Id = id,
                Tags = ["a"],
                Data = data
            };
        }
    }
}
