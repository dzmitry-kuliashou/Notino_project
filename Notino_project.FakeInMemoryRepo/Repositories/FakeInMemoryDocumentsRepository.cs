using Notino_project.FakeInMemoryRepo.Mappers;
using Notino_project.FakeInMemoryRepo.StorageModels;
using Notino_project.Core.Aggregates;
using Notino_project.Repositories.Interfaces.Repositories;

namespace Notino_project.FakeInMemoryRepo.Repositories
{
    public class FakeInMemoryDocumentsRepository : IDocumentsRepository
    {
        public Task<Document> GetById(Guid id)
        {
            var existingDocument = TryGetExistingDocument(id.ToString());

            if (existingDocument == null)
            {
                return Task.FromResult<Document>(null);
            }

            return Task.FromResult(existingDocument.ToAggregateDocument());
        }

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

            var existingDocument = TryGetExistingDocument(document.Id.ToString());
            
            if (existingDocument == null)
            {
                throw new Exception($"Document with Id='{document.Id}' does not exist.");
            }
            
            existingDocument.Tags = document.Tags;
            existingDocument.Data = document.Data;

            return Task.FromResult(existingDocument.ToAggregateDocument());
        }

        private DocumentDal? TryGetExistingDocument(string id)
        {
            DocumentsStorage.Documents.TryGetValue(id, out var existingDocument);

            return existingDocument;
        }
    }
}
