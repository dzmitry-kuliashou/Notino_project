using Notino_project.Contract.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace Notino_project.Contract.Examples;

public class UpdateDocumentExample : IExamplesProvider<UpdateDocumentRequest>
{
    public UpdateDocumentRequest GetExamples() =>
        new()
        {
            Tags = ["example"],
            Data = new Dictionary<string, string> { ["name"] = "updated example" }
        };
}
