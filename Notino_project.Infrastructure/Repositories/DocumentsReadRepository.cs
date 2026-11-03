using MongoDB.Driver;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.Mappers;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.Infrastructure.Repositories;

public sealed class DocumentsReadRepository(IMongoCollection<MongoDocument> collection)
    : IDocumentsReadRepository
{
    public async Task<Core.Aggregates.Document?> GetById(Guid id)
    {
        var document = await collection
            .Find(storedDocument => storedDocument.Id == id)
            .FirstOrDefaultAsync();

        return document?.ToCore();
    }
}
