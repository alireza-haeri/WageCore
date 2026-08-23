namespace Core.Contracts;

public record PaginationDto(int PageNumber, int PageSize)
{
    public int Offset => (PageNumber - 1) * PageSize;
};

public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector)
        => new(Items.Select(selector).ToList(), TotalCount, PageNumber, PageSize);
}