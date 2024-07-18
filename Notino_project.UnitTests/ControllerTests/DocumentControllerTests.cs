using Microsoft.AspNetCore.Mvc;
using Moq;
using Notino_project.Controllers;
using Notino_project.Dtos.Document;
using Notino_project.Models.Models;
using Notino_project.Services.Interfaces.Services.Documents;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ControllerTests
{
    public class DocumentControllerTests
    {
        [Fact]
        public async Task GetById_DocumentDoesNotExist_ReturnsNotFound()
        {
            var mockDocumentsService = new Mock<IDocumentsService>();

            var controller = new DocumentsController(mockDocumentsService.Object);

            var result = await controller.GetById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_DocumentExists_ReturnsDocumentDto()
        {
            var id = Guid.NewGuid();

            var mockDocumentsService = new Mock<IDocumentsService>();
            mockDocumentsService.Setup(m => m.GetById(id)).ReturnsAsync(GenerateValidDocument(id));

            var controller = new DocumentsController(mockDocumentsService.Object);

            var result = await controller.GetById(id);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<DocumentDto>(((OkObjectResult)result).Value);
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

        //We can similarly test another Controller's methods
    }
}
