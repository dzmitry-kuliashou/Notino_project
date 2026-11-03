using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notino_project.Application.Commands;
using Notino_project.Application.Queries;
using Notino_project.Contract.Requests;

namespace Notino_project.Controllers
{
    [ApiController]
    [Route("/documents")]
    public class DocumentController(ISender sender) : Controller
    {
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id) =>
            await sender.Send(new GetByIdQuery(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDocumentRequest createDocumentModel) =>
            await sender.Send(new CreateDocumentCommand(createDocumentModel));

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateDocumentRequest updateDocumentModel) =>
            await sender.Send(new UpdateDocumentCommand(id, updateDocumentModel));
    }
}
