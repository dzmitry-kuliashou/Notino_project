using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Notino_project.Application.Caching;
using Notino_project.Application.Mappers;
using Notino_project.Contract.Requests;
using Notino_project.Core.Aggregates;
using Notino_project.Repositories.Interfaces.Repositories;

namespace Notino_project.Application.Commands
{
    public sealed record CreateDocumentCommand(CreateDocumentRequest Request) : IRequest<IActionResult>;

    public sealed class CreateDocumentCommandHandler(
        IDocumentsRepository repository,
        IValidator<CreateDocumentRequest> validator,
        IMemoryCache cache) : IRequestHandler<CreateDocumentCommand, IActionResult>
    {
        public async Task<IActionResult> Handle(CreateDocumentCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command.Request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.Select(error => error.ErrorMessage).ToArray());
            }

            var document = Document.Construct(command.Request.Tags, command.Request.Data);
            var created = await repository.Create(document);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DocumentCachePolicy.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = DocumentCachePolicy.SlidingExpiration
            };
            cache.Set(DocumentCacheKeys.For(created.Id), created, cacheEntryOptions);

            return new CreatedAtActionResult(
                "Create",
                null,
                new { id = created.Id },
                created.ToResponse());
        }
    }
}
