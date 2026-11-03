namespace Notino_project.Contract.Responses;

public class DocumentResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}
