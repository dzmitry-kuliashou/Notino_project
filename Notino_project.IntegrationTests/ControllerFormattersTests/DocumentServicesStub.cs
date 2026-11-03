using Notino_project.Core.Aggregates;
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

            JsonElement data;
            using (var doc = JsonDocument.Parse(jsonInput))
            {
                var root = doc.RootElement;
                data = root.GetProperty("data").Clone();
            }

            var document = Document.Construct(["a"], data);
            document.Id = id;
            return document;
        }
    }
}
