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
        var result = await collection.ReplaceOneAsync(
            storedDocument => storedDocument.Id == document.Id,
            document.ToMongoModel(),
            new ReplaceOptions { IsUpsert = false });

        if (result.MatchedCount == 0)
        {
            throw new DocumentNotFoundException(document.Id);
        }

        return document;
    }
}
