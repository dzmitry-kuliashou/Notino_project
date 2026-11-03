using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Notino_project.Application.Caching;
using Notino_project.Application.Mappers;
using Notino_project.Contract.Requests;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;

namespace Notino_project.Application.Commands
{
    public sealed record UpdateDocumentCommand(Guid Id, UpdateDocumentRequest Request) : IRequest<IActionResult>;

    public sealed class UpdateDocumentCommandHandler(
        IDocumentsReadRepository readRepository,
        IDocumentsWriteRepository writeRepository,
        IValidator<UpdateDocumentRequest> validator,
        IMemoryCache cache) : IRequestHandler<UpdateDocumentCommand, IActionResult>
    {
        public async Task<IActionResult> Handle(UpdateDocumentCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command.Request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.Select(error => error.ErrorMessage).ToArray());
            }

            var cacheKey = DocumentCacheKeys.For(command.Id);
            if (!cache.TryGetValue(cacheKey, out Document? existing))
            {
                existing = await readRepository.GetById(command.Id);

                if (existing is not null)
                {
                    cache.Set(cacheKey, existing, CreateCacheEntryOptions());
                }
            }

            if (existing is null)
            {
                return new NotFoundResult();
            }

            existing.Update(command.Request.Tags, command.Request.Data);
            var updated = await writeRepository.Update(existing);
            cache.Set(cacheKey, updated, CreateCacheEntryOptions());

            return new OkObjectResult(updated.ToResponse());
        }

        private static MemoryCacheEntryOptions CreateCacheEntryOptions() =>
            new()
            {
                AbsoluteExpirationRelativeToNow = DocumentCachePolicy.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = DocumentCachePolicy.SlidingExpiration
            };
    }
}
