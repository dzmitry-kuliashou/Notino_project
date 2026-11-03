using Notino_project.Infrastructure.FakeInMemoryStorage.Mappers;
using Notino_project.Infrastructure.FakeInMemoryStorage.StorageModels;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;

namespace Notino_project.Infrastructure.FakeInMemoryStorage.Repositories
{
    public class FakeInMemoryDocumentsWriteRepository : IDocumentsWriteRepository
    {
        public Task<Document> Create(Document document)
        {
            var documentDal = document.ToDocumentDal();

            DocumentsStorage.Documents.TryAdd(document.Id.ToString(), documentDal);

            return Task.FromResult(document);
        }

        public Task<Document> Update(Document document)
        {
            if (document.Id == Guid.Empty)
            {
                throw new Exception("Document Id is not defined.");
            }

            if (!DocumentsStorage.Documents.TryGetValue(document.Id.ToString(), out var existingDocument))
            {
                throw new Exception($"Document with Id='{document.Id}' does not exist.");
            }

            existingDocument.Tags = document.Tags;
            existingDocument.Data = document.Data;

            return Task.FromResult(existingDocument.ToAggregateDocument());
        }
    }
}
