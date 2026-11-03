using Notino_project.Dtos.Document;
using Notino_project.Core.Aggregates;
using System.Text.Json;

namespace Notino_project.Mappers
{
    public static class DocumentMappers
    {
        public static DocumentDto ToDocumentDto(this Document document) =>
            new()
            {
                Id = document.Id,
                Tags = document.Tags,
                Data = JsonSerializer.Deserialize<Dictionary<string, string>>(document.Data.GetRawText()),
            };

    }
}
