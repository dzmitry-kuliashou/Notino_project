using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;
using System.Text.Json;

namespace Notino_project.IntegrationTests.ControllerFormattersTests
{
    internal sealed class DocumentsRepositoryStub : IDocumentsReadRepository, IDocumentsWriteRepository
    {
        public static Guid ExistingDocumentId = Guid.NewGuid();

        public Task<Document?> GetById(Guid id)
        {
            if (id != ExistingDocumentId)
            {
                return Task.FromResult<Document?>(null);
            }

            using var jsonDocument = JsonDocument.Parse("""{"something":"This is data"}""");
            var document = Document.Construct(["a"], jsonDocument.RootElement.Clone());
            document.Id = id;
            return Task.FromResult<Document?>(document);
        }

        public Task<(IReadOnlyList<Document> Documents, long TotalCount)> List(int page, int pageSize) =>
            Task.FromResult<(IReadOnlyList<Document> Documents, long TotalCount)>((Array.Empty<Document>(), 0));

        public Task<Document> Create(Document document) =>
            Task.FromException<Document>(new NotImplementedException());

        public Task<Document> Update(Document document) =>
            Task.FromResult(document);
    }
}
