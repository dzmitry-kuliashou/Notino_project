using MongoDB.Driver;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.Mappers;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;
using CoreDocument = Notino_project.Core.Aggregates.Document;

namespace Notino_project.Infrastructure.Repositories;

public sealed class DocumentsReadRepository(IMongoCollection<MongoDocument> collection)
    : ReadRepository<CoreDocument, MongoDocument>(collection), IDocumentsReadRepository
{
    protected override CoreDocument ToAggregate(MongoDocument storageModel) =>
        storageModel.ToCore();
}
