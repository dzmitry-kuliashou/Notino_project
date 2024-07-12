using Microsoft.AspNetCore.Mvc;
using Notino_project.Dtos.Document;
using Notino_project.Mappers;
using Notino_project.Services.Interfaces.Services.Documents;

namespace Notino_project.Controllers
{
    [ApiController]
    [Route("/documents")]
    public class DocumentsController : Controller
    {
        private readonly IDocumentsService _documentsService;

        public DocumentsController(IDocumentsService documentsService)
        {
            _documentsService = documentsService;
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var document = await _documentsService.GetById(id);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document.ToDocumentDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDocumentDto createDocumentModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var document = createDocumentModel.ToDocumentFromCreateDto();

            var created = await _documentsService.Create(document);

            return Ok(created.ToDocumentDto());
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDocumentDto updateDocumentModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _documentsService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            var document = updateDocumentModel.ToDocumentFromUpdateDto(id);
            var updated = await _documentsService.Update(document);

            return Ok(updated.ToDocumentDto());
        }
    }
}
