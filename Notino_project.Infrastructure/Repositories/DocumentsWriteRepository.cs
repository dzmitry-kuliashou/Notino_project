using MongoDB.Driver;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.Mappers;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;
using CoreDocument = Notino_project.Core.Aggregates.Document;

namespace Notino_project.Infrastructure.Repositories;

public sealed class DocumentsWriteRepository(IMongoCollection<MongoDocument> collection)
    : WriteRepository<CoreDocument, MongoDocument>(collection), IDocumentsWriteRepository
{
    protected override MongoDocument ToStorageModel(CoreDocument aggregate) =>
        aggregate.ToMongoModel();
}
