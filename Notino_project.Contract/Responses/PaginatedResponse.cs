namespace Notino_project.Contract.Responses;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    public int TotalPageCount { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public bool HasPrevious => CurrentPage > 0;

    public bool HasNext => CurrentPage < TotalPageCount - 1;
}
