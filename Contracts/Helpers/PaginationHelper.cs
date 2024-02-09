using Contracts.Filters;
using Contracts.Wrappers;

namespace Contracts.Helpers;

public static class PaginationHelper
{
    public static PagedResponse<IEnumerable<T>> CreatePagedResponse<T>(IEnumerable<T> pagedData, PaginationFilter validFilter, int totalRecords)
    {
        var response = new PagedResponse<IEnumerable<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
        var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
        var roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

        response.TotalPages = roundedTotalPages;
        response.TotalRecords = totalRecords;
        return response;
    }
}