using Notino_project.Models.Models;
using Notino_project.Services.Interfaces.Services.Documents;
using System.Text.Json;

namespace Notino_project.IntegrationTests.ControllerFormattersTests
{
    internal class DocumentsServiceStub : IDocumentsService
    {
        public static Guid ExistingDocumentId = Guid.NewGuid();

        public Task<Document> GetById(Guid id)
        {
            if (id == ExistingDocumentId)
            {
                return Task.FromResult(GenerateValidDocument(id));
            }

            return Task.FromResult<Document>(null);
        }

        public Task<Document> Create(Document document)
        {
            throw new NotImplementedException();
        }

        public Task<Document> Update(Document document)
        {
            throw new NotImplementedException();
        }

        private Document GenerateValidDocument(Guid id)
        {
            var jsonInput = @"{
                        ""data"": 
                        {
                            ""something"": ""This is data""
                        }
                    }";

            var doc = JsonDocument.Parse(jsonInput);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            return new Document
            {
                Id = id,
                Tags = ["a"],
                Data = data
            };
        }
    }
}
