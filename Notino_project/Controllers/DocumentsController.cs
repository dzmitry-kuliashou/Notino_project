using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Notino_project.Contract.Requests;
using Notino_project.Core.Aggregates;
using Notino_project.Mappers;
using Notino_project.Services.Interfaces.Services.Documents;

namespace Notino_project.Controllers
{
    [ApiController]
    [Route("/documents")]
    public class DocumentsController : Controller
    {
        private readonly IDocumentsService _documentsService;
        private readonly IValidator<CreateDocumentRequest> _createDocumentValidator;
        private readonly IValidator<UpdateDocumentRequest> _updateDocumentValidator;

        public DocumentsController(
            IDocumentsService documentsService,
            IValidator<CreateDocumentRequest> createDocumentValidator,
            IValidator<UpdateDocumentRequest> updateDocumentValidator)
        {
            _documentsService = documentsService;
            _createDocumentValidator = createDocumentValidator;
            _updateDocumentValidator = updateDocumentValidator;
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

            return Ok(document.ToResponse());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDocumentRequest createDocumentModel)
        {
            var validationResult = await _createDocumentValidator.ValidateAsync(createDocumentModel);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToArray());
            }

            var document = Document.Construct(createDocumentModel.Tags, createDocumentModel.Data);

            var created = await _documentsService.Create(document);

            return CreatedAtAction(nameof(Create), new { id = created.Id }, created.ToResponse());
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDocumentRequest updateDocumentModel)
        {
            var validationResult = await _updateDocumentValidator.ValidateAsync(updateDocumentModel);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToArray());
            }

            var existing = await _documentsService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Update(updateDocumentModel.Tags, updateDocumentModel.Data);
            var updated = await _documentsService.Update(existing);

            return Ok(updated.ToResponse());
        }
    }
}
