using Notino_project.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Notino_project.Dtos.Document
{
    public class CreateDocumentDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Tags collection shouldn't be empty")]
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

        [Required]
        [SimplePropertiesOnly]
        public JsonElement Data { get; set; }
    }
}
