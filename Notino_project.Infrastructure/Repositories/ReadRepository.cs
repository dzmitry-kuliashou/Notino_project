using MongoDB.Driver;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;
using Notino_project.Infrastructure.StorageModels;

namespace Notino_project.Infrastructure.Repositories;

public abstract class ReadRepository<TAggregate, TStorageModel>(
    IMongoCollection<TStorageModel> collection) : IReadRepository<TAggregate>
    where TAggregate : AggregateRoot
    where TStorageModel : StorageModel
{
    protected abstract TAggregate ToAggregate(TStorageModel storageModel);

    public async Task<TAggregate?> GetById(Guid id)
    {
        var filter = Builders<TStorageModel>.Filter.And(
            Builders<TStorageModel>.Filter.Eq(storageModel => storageModel.Id, id),
            Builders<TStorageModel>.Filter.Ne(storageModel => storageModel.IsDeleted, true));
        var storageModel = await collection
            .Find(filter)
            .FirstOrDefaultAsync();

        return storageModel is null ? null : ToAggregate(storageModel);
    }

    public async Task<(IReadOnlyList<TAggregate> Documents, long TotalCount)> List(
        int page,
        int pageSize)
    {
        var filter = Builders<TStorageModel>.Filter.Ne(storageModel => storageModel.IsDeleted, true);
        var totalCount = await collection.CountDocumentsAsync(filter);
        var storageModels = await collection
            .Find(filter)
            .Sort(Builders<TStorageModel>.Sort.Ascending(storageModel => storageModel.Id))
            .Skip(checked(page * pageSize))
            .Limit(pageSize)
            .ToListAsync();

        return ([.. storageModels.Select(ToAggregate)], totalCount);
    }
}
