using MongoDB.Driver;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.Mappers;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.Infrastructure.Repositories;

public sealed class DocumentsWriteRepository(IMongoCollection<MongoDocument> collection)
    : IDocumentsWriteRepository
{
    public async Task<Document> Create(Document document)
    {
        try
        {
            await collection.InsertOneAsync(document.ToMongoModel());
        }
        catch (MongoWriteException exception) when (
            exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DocumentAlreadyExistsException(document.Id);
        }

        return document;
    }

    public async Task<Document> Update(Document document)
    {
        var filter = Builders<MongoDocument>.Filter.And(
            Builders<MongoDocument>.Filter.Eq(storedDocument => storedDocument.Id, document.Id),
            Builders<MongoDocument>.Filter.Ne(storedDocument => storedDocument.IsDeleted, true));
        var result = await collection.ReplaceOneAsync(
            filter,
            document.ToMongoModel(),
            new ReplaceOptions { IsUpsert = false });

        if (result.MatchedCount == 0)
        {
            throw new DocumentNotFoundException(document.Id);
        }

        return document;
    }
}
