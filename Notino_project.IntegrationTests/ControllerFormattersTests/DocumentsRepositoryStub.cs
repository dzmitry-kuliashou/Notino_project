using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;

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

            var document = Document.Construct(
                ["a"],
                new Dictionary<string, string> { ["something"] = "This is data" });
            document.Id = id;
            return Task.FromResult<Document?>(document);
        }

        public Task<(IReadOnlyList<Document> Documents, long TotalCount)> List(int page, int pageSize) =>
            Task.FromResult<(IReadOnlyList<Document> Documents, long TotalCount)>((Array.Empty<Document>(), 0));

        public Task<Document> Create(Document document) =>
            Task.FromException<Document>(new NotImplementedException());

        public Task<Document> Update(Document document) =>
            Task.FromResult(document);

        public Task Delete(Document document)
        {
            document.Delete();
            return Task.CompletedTask;
        }
    }
}
