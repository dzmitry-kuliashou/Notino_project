using Notino_project.Core.Aggregates;

namespace Notino_project.Core.Repositories;

public interface IWriteRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    Task<TAggregate> Create(TAggregate aggregate);

    Task<TAggregate> Update(TAggregate aggregate);

    Task Delete(TAggregate aggregate);
}
