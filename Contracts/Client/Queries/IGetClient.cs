using Contracts.Client.Queries.Models;
using Contracts.Filters;
using Contracts.Models;
using Contracts.Wrappers;

namespace Contracts.Client.Queries;

public interface IGetClient
{
    Task<ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>>> Get(PaginationFilter paginationFilter, CancellationToken cancellationToken);
}