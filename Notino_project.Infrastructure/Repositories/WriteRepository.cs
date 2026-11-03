using MongoDB.Driver;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.StorageModels;

namespace Notino_project.Infrastructure.Repositories;

public abstract class WriteRepository<TAggregate, TStorageModel>(
    IMongoCollection<TStorageModel> collection) : IWriteRepository<TAggregate>
    where TAggregate : AggregateRoot
    where TStorageModel : StorageModel
{
    protected abstract TStorageModel ToStorageModel(TAggregate aggregate);

    public async Task<TAggregate> Create(TAggregate aggregate)
    {
        try
        {
            await collection.InsertOneAsync(ToStorageModel(aggregate));
        }
        catch (MongoWriteException exception) when (
            exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new AggregateAlreadyExistsException(aggregate.Id);
        }

        return aggregate;
    }

    public async Task<TAggregate> Update(TAggregate aggregate)
    {
        var filter = Builders<TStorageModel>.Filter.And(
            Builders<TStorageModel>.Filter.Eq(storageModel => storageModel.Id, aggregate.Id),
            Builders<TStorageModel>.Filter.Ne(storageModel => storageModel.IsDeleted, true));
        var result = await collection.ReplaceOneAsync(
            filter,
            ToStorageModel(aggregate),
            new ReplaceOptions { IsUpsert = false });

        if (result.MatchedCount == 0)
        {
            throw new AggregateNotFoundException(aggregate.Id);
        }

        return aggregate;
    }

    public async Task Delete(TAggregate aggregate)
    {
        aggregate.Delete();
        await Update(aggregate);
    }
}
