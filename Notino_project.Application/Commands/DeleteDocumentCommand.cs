using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Notino_project.Application.Caching;
using Notino_project.Core.Exceptions;
using Notino_project.Core.Repositories;

namespace Notino_project.Application.Commands
{
    public sealed record DeleteDocumentCommand(Guid Id) : IRequest<IActionResult>;

    public sealed class DeleteDocumentCommandHandler(
        IDocumentsReadRepository readRepository,
        IDocumentsWriteRepository writeRepository,
        IMemoryCache cache) : IRequestHandler<DeleteDocumentCommand, IActionResult>
    {
        public async Task<IActionResult> Handle(
            DeleteDocumentCommand command,
            CancellationToken cancellationToken)
        {
            var cacheKey = DocumentCacheKeys.For(command.Id);
            var document = await readRepository.GetById(command.Id);
            if (document is null)
            {
                cache.Remove(cacheKey);
                return new NotFoundResult();
            }

            document.Delete();
            try
            {
                await writeRepository.Update(document);
            }
            catch (DocumentNotFoundException)
            {
                cache.Remove(cacheKey);
                return new NotFoundResult();
            }

            cache.Remove(cacheKey);
            return new NoContentResult();
        }
    }
}
