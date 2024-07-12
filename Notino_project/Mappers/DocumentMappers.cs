using Notino_project.Dtos.Document;
using Notino_project.Models.Models;
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

        public static Document ToDocumentFromCreateDto(this CreateDocumentDto documentDto) =>
            new()
            {
                Tags = documentDto.Tags,
                Data = documentDto.Data,
            };

        public static Document ToDocumentFromUpdateDto(this UpdateDocumentDto documentDto, Guid id) =>
            new()
            {
                Id = id,
                Tags = documentDto.Tags,
                Data = documentDto.Data,
            };
    }
}
