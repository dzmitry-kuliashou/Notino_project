using Notino_project.Infrastructure.FakeInMemoryStorage.StorageModels;
using Notino_project.Core.Aggregates;

namespace Notino_project.Infrastructure.FakeInMemoryStorage.Mappers
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
