using System.Text.Json;

namespace Notino_project.Contract.Requests;

public class CreateDocumentRequest
{
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    public JsonElement Data { get; set; }
}
