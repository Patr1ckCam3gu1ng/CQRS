namespace Contracts.Filters;

public class PaginationFilter
{
    public PaginationFilter()
    {

    }
    
    public PaginationFilter(int pageNumber, int pageSize, string q)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize == 0 ? 10 : pageSize;
        this.q = !string.IsNullOrWhiteSpace(q) ? q : "";
    }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 5;

    public string? q { get; set; }
}