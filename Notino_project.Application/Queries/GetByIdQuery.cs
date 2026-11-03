using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Notino_project.Application.Caching;
using Notino_project.Application.Mappers;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;

namespace Notino_project.Application.Queries
{
    public sealed record GetByIdQuery(Guid Id) : IRequest<IActionResult>;

    public sealed class GetByIdQueryHandler(
        IDocumentsReadRepository repository,
        IMemoryCache cache) : IRequestHandler<GetByIdQuery, IActionResult>
    {
        public async Task<IActionResult> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = DocumentCacheKeys.For(request.Id);
            if (!cache.TryGetValue(cacheKey, out Document? document))
            {
                document = await repository.GetById(request.Id);

                if (document is not null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = DocumentCachePolicy.AbsoluteExpirationRelativeToNow,
                        SlidingExpiration = DocumentCachePolicy.SlidingExpiration
                    };
                    cache.Set(cacheKey, document, cacheEntryOptions);
                }
            }

            if (document is null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(document.ToResponse());
        }
    }
}
