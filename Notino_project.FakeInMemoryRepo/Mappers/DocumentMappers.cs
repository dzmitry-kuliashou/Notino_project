using Notino_project.FakeInMemoryRepo.StorageModels;
using Notino_project.Models.Models;

namespace Notino_project.FakeInMemoryRepo.Mappers
{
    public static class DocumentMappers
    {
        public static Document ToDocument(this DocumentDal documentDal) =>
            new()
            {
                Id = documentDal.Id,
                Tags = documentDal.Tags,
                Data = documentDal.Data,
            };

        public static DocumentDal ToDocumentDal(this Document document) =>
            new()
            {
                Id = document.Id,
                Tags = document.Tags,
                Data = document.Data,
            };
    }
}
