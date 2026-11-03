using Notino_project.FakeInMemoryRepo.StorageModels;
using Notino_project.Core.Aggregates;

namespace Notino_project.FakeInMemoryRepo.Mappers
{
    public static class DocumentMappers
    {
        public static Document ToAggregateDocument(this DocumentDal documentDal)
        {
            var document = Document.Construct(documentDal.Tags, documentDal.Data);
            document.Id = documentDal.Id;
            return document;
        }

        public static DocumentDal ToDocumentDal(this Document document) =>
            new()
            {
                Id = document.Id,
                Tags = document.Tags,
                Data = document.Data,
            };
    }
}
