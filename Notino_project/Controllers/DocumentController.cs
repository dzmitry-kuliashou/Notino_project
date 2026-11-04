using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notino_project.Application.Commands;
using Notino_project.Application.Queries;
using Notino_project.Contract.Examples;
using Notino_project.Contract.Requests;
using Notino_project.Contract.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace Notino_project.Controllers
{
    [ApiController]
    [Route("/documents")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class DocumentController(ISender sender) : Controller
    {
        [HttpGet]
        [Produces("application/json", "application/xml", "text/xml", "application/x-msgpack")]
        [ProducesResponseType(typeof(DocumentsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] ListParams parameters) =>
            await sender.Send(new ListDocumentsQuery(parameters));

        [HttpGet("{id:Guid}")]
        [Produces("application/json", "application/xml", "text/xml", "application/x-msgpack")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id) =>
            await sender.Send(new GetByIdQuery(id));

        [HttpPost]
        [Produces("application/json", "application/xml", "text/xml", "application/x-msgpack")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CreateDocumentRequest), typeof(CreateDocumentExample))]
        public async Task<IActionResult> Create([FromBody] CreateDocumentRequest createDocumentModel) =>
            await sender.Send(new CreateDocumentCommand(createDocumentModel));

        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id) =>
            await sender.Send(new DeleteDocumentCommand(id));

        [HttpPut("{id:Guid}")]
        [Produces("application/json", "application/xml", "text/xml", "application/x-msgpack")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateDocumentRequest), typeof(UpdateDocumentExample))]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateDocumentRequest updateDocumentModel) =>
            await sender.Send(new UpdateDocumentCommand(id, updateDocumentModel));
    }
}
