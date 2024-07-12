using Notino_project.Models.Models;

namespace Notino_project.Services.Interfaces.Services.Documents
{
    public interface IDocumentsService
    {
        Task<Document> GetById(Guid id);
        Task<Document> Create(Document document);
        Task<Document> Update(Document document);
    }
}
