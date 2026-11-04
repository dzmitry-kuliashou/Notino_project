namespace Notino_project.Contract.Requests;

public class CreateDocumentRequest
{
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    public Dictionary<string, string> Data { get; set; } = new();
}
