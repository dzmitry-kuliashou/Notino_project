using Notino_project.Infrastructure.FakeInMemoryStorage.Mappers;
using Notino_project.Infrastructure.FakeInMemoryStorage.StorageModels;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;

namespace Notino_project.Infrastructure.FakeInMemoryStorage.Repositories
{
    public class FakeInMemoryDocumentsReadRepository : IDocumentsReadRepository
    {
        public Task<Document?> GetById(Guid id)
        {
            DocumentsStorage.Documents.TryGetValue(id.ToString(), out var existingDocument);

            if (existingDocument == null)
            {
                return Task.FromResult<Document?>(null);
            }

            return Task.FromResult<Document?>(existingDocument.ToAggregateDocument());
        }
    }
}
