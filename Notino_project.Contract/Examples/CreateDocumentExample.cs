using Notino_project.Contract.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace Notino_project.Contract.Examples;

public class CreateDocumentExample : IExamplesProvider<CreateDocumentRequest>
{
    public CreateDocumentRequest GetExamples() =>
        new()
        {
            Tags = ["example"],
            Data = new Dictionary<string, string> { ["name"] = "example" }
        };
}
