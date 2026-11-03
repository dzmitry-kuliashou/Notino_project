using Notino_project.Core.Aggregates;

namespace Notino_project.Core.Repositories
{
    public interface IDocumentsWriteRepository
    {
        Task<Document> Create(Document document);
        Task<Document> Update(Document document);
    }
}
