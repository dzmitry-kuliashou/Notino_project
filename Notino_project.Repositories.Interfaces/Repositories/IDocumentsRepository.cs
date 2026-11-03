using Notino_project.Core.Aggregates;

namespace Notino_project.Repositories.Interfaces.Repositories
{
    public interface IDocumentsRepository
    {
        Task<Document> GetById(Guid id);
        Task<Document> Create(Document document);
        Task<Document> Update(Document document);
    }
}
