using Notino_project.Contract.Responses;
using Notino_project.Core.Aggregates;

namespace Notino_project.Application.Mappers
{
    public static class DocumentMappers
    {
        public static DocumentResponse ToResponse(this Document document) =>
            new()
            {
                Id = document.Id,
                Tags = document.Tags,
                Data = document.Data.ToDictionary(
                    property => property.Key,
                    property => property.Value,
                    StringComparer.Ordinal),
            };
    }
}
