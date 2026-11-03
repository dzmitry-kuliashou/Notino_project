using Notino_project.Core.Aggregates;
using Notino_project.Repositories.Interfaces.Repositories;
using System.Text.Json;

namespace Notino_project.IntegrationTests.ControllerFormattersTests
{
    internal sealed class DocumentsRepositoryStub : IDocumentsRepository
    {
        public static Guid ExistingDocumentId = Guid.NewGuid();

        public Task<Document> GetById(Guid id)
        {
            if (id != ExistingDocumentId)
            {
                return Task.FromException<Document>(new KeyNotFoundException($"Document {id} was not found."));
            }

            using var jsonDocument = JsonDocument.Parse("""{"something":"This is data"}""");
            var document = Document.Construct(["a"], jsonDocument.RootElement.Clone());
            document.Id = id;
            return Task.FromResult(document);
        }

        public Task<Document> Create(Document document) =>
            Task.FromException<Document>(new NotImplementedException());

        public Task<Document> Update(Document document) =>
            Task.FromException<Document>(new NotImplementedException());
    }
}
