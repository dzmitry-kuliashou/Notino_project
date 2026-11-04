using Notino_project.Core.Aggregates;
using DocumentMongoModel = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.Infrastructure.Mappers;

public static class DocumentMapper
{
    public static Document ToCore(this DocumentMongoModel model)
    {
        var document = Document.Construct(model.Tags, model.Data);
        document.Id = model.Id;
        if (model.IsDeleted)
        {
            document.Delete();
        }

        return document;
    }

    public static DocumentMongoModel ToMongoModel(this Document document)
    {
        return new DocumentMongoModel
        {
            Id = document.Id,
            Tags = document.Tags.ToList(),
            Data = document.Data.ToDictionary(
                property => property.Key,
                property => property.Value,
                StringComparer.Ordinal),
            IsDeleted = document.IsDeleted,
        };
    }
}