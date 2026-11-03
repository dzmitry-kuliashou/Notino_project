using MongoDB.Driver;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.Mappers;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;
using CoreDocument = Notino_project.Core.Aggregates.Document;

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

    public async Task<(IReadOnlyList<CoreDocument> Documents, long TotalCount)> List(int page, int pageSize)
    {
        var filter = Builders<MongoDocument>.Filter.Empty;
        var totalCount = await collection.CountDocumentsAsync(filter);
        var documents = await collection
            .Find(filter)
            .SortBy(document => document.Id)
            .Skip(checked(page * pageSize))
            .Limit(pageSize)
            .ToListAsync();

        return (documents.Select(document => document.ToCore()).ToArray(), totalCount);
    }
}
