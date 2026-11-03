using Notino_project.Core.Aggregates;

namespace Notino_project.Core.Repositories
{
    public interface IDocumentsReadRepository
    {
        Task<Document?> GetById(Guid id);

        Task<(IReadOnlyList<Document> Documents, long TotalCount)> List(int page, int pageSize);
    }
}
