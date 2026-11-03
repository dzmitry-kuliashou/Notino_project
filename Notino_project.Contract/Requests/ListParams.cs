using Microsoft.AspNetCore.Mvc;

namespace Notino_project.Contract.Requests;

public class ListParams
{
    [FromQuery(Name = "p")]
    public string Page { get; set; } = "0";

    [FromQuery(Name = "ps")]
    public string PageSize { get; set; } = "10";
}
