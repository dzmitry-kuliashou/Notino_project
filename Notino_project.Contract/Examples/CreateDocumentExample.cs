using System.Text.Json;
using Notino_project.Contract.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace Notino_project.Contract.Examples;

public class CreateDocumentExample : IExamplesProvider<CreateDocumentRequest>
{
    public CreateDocumentRequest GetExamples() =>
        new()
        {
            Tags = ["example"],
            Data = JsonSerializer.SerializeToElement(new { name = "example" })
        };
}
