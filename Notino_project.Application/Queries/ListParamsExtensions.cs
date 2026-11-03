using System.Globalization;
using Notino_project.Contract.Requests;

namespace Notino_project.Application.Queries;

public static class ListParamsExtensions
{
    public static int GetNormalizedPage(this ListParams parameters)
    {
        var page = int.TryParse(
            parameters.Page,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var parsedPage) && parsedPage >= 0
                ? parsedPage
                : 0;

        return page > int.MaxValue / parameters.GetNormalizedPageSize() ? int.MaxValue / parameters.GetNormalizedPageSize() : page;
    }

    public static int GetNormalizedPageSize(this ListParams parameters) =>
        int.TryParse(
            parameters.PageSize,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var parsedPageSize) && parsedPageSize > 0
                ? parsedPageSize
                : 10;
}
