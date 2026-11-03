using Notino_project.Contract.Responses;
using Notino_project.Core.Aggregates;
using System.Text.Json;

namespace Notino_project.Application.Mappers
{
    public static class DocumentMappers
    {
        public static DocumentResponse ToResponse(this Document document) =>
            new()
            {
                Id = document.Id,
                Tags = document.Tags,
                Data = JsonSerializer.Deserialize<Dictionary<string, string>>(document.Data.GetRawText())
                    ?? throw new InvalidOperationException("Document data must be a JSON object."),
            };
    }
}
