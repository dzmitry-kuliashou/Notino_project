using System.Text.Json;
using Notino_project.Core.Aggregates;
using DocumentMongoModel = Notino_project.Infrastructure.StorageModels.Document;

namespace Notino_project.Infrastructure.Mappers;

public static class DocumentMapper
{
    public static Document ToCore(this DocumentMongoModel model)
    {
        var document = Document.Construct(model.Tags, JsonSerializer.SerializeToElement(model.Data));
        document.Id = model.Id;
        return document;
    }

    public static DocumentMongoModel ToMongoModel(this Document document)
    {
        var data = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in document.Data.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String ||
                property.Value.GetString() is not { } value)
            {
                throw new InvalidOperationException("Document data must contain only string values.");
            }

            data.Add(property.Name, value);
        }

        return new DocumentMongoModel
        {
            Id = document.Id,
            Tags = document.Tags.ToList(),
            Data = data,
        };
    }
}