using Notino_project.Core.Aggregates;

namespace Notino_project.Core.Repositories;

public interface IReadRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    Task<TAggregate?> GetById(Guid id);

    Task<(IReadOnlyList<TAggregate> Documents, long TotalCount)> List(int page, int pageSize);
}
