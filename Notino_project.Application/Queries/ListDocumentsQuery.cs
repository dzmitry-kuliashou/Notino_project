using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notino_project.Application.Mappers;
using Notino_project.Contract.Requests;
using Notino_project.Contract.Responses;
using Notino_project.Core.Repositories;

namespace Notino_project.Application.Queries;

public sealed record ListDocumentsQuery(ListParams Parameters) : IRequest<IActionResult>;

public sealed class ListDocumentsQueryHandler(
    IDocumentsReadRepository repository) : IRequestHandler<ListDocumentsQuery, IActionResult>
{
    public async Task<IActionResult> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Parameters.GetNormalizedPage();
        var pageSize = request.Parameters.GetNormalizedPageSize();

        var (documents, totalCount) = await repository.List(page, pageSize);
        var totalPageCount = checked((int)(totalCount / pageSize + (totalCount % pageSize == 0 ? 0 : 1)));

        return new OkObjectResult(new DocumentsResponse
        {
            Items = documents.Select(document => document.ToResponse()).ToArray(),
            TotalPageCount = totalPageCount,
            CurrentPage = page,
            PageSize = pageSize
        });
    }
}
