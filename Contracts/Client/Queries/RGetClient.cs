using System.Text.Json;
using AutoMapper;
using Broker.Cache;
using Contracts.Client.Queries.Models;
using Contracts.Filters;
using Contracts.Helpers;
using Contracts.Models;
using Contracts.Repositories.Client;
using Contracts.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared;

namespace Contracts.Client.Queries;

public class RGetClient : IGetClient
{
    private readonly IMapper _mapper;
    private readonly IClientRepository _client;
    private readonly ILogger<RGetClient> _log;
    private readonly ICacheService _cache;

    private static readonly string ClientCacheKey = CacheKeys.Client;

    public RGetClient(IMapper mapper, IClientRepository client, ILogger<RGetClient> log, ICacheService cache)
    {
        _mapper = mapper;
        _client = client;
        _log = log;
        _cache = cache;
    }

    public async Task<ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>>> Get(PaginationFilter paginationFilter, CancellationToken cancellationToken)
    {
        ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>> responseModel = new();

        try
        {
            var isCached = await _cache.TryGet(ClientCacheKey, out var clientsFromCache, cancellationToken);
            if (isCached && string.IsNullOrEmpty(clientsFromCache) == false && string.IsNullOrEmpty(paginationFilter.q))
            {
                return ConstructResponseBody(paginationFilter, JsonSerializer.Deserialize<List<GetClientResponseModel>>(clientsFromCache), responseModel);
            }

            var clients = await _client.Get(paginationFilter.q, cancellationToken);

            var mappedResponse = _mapper.Map<List<GetClientResponseModel>>(clients);

            if (string.IsNullOrEmpty(paginationFilter.q) && mappedResponse.Any())
            {
                await _cache.Set(ClientCacheKey, JsonSerializer.Serialize(mappedResponse), cancellationToken);
            }

            return ConstructResponseBody(paginationFilter, mappedResponse, responseModel);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in RGetClient -> GetAll");
            responseModel.StatusCode = StatusCodes.Status500InternalServerError;
            responseModel.Message = ex.Message;
            return responseModel;
        }

        throw new NotImplementedException();
    }

    private static ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>> ConstructResponseBody(PaginationFilter paginationFilter,
        IReadOnlyCollection<GetClientResponseModel>? mappedResponse, ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>> responseModel)
    {
        var totalItems = mappedResponse.Count;

        var paginatedResults = mappedResponse.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Take(paginationFilter.PageSize);

        var pagedResponse = PaginationHelper.CreatePagedResponse(paginatedResults, paginationFilter, totalItems);

        responseModel.StatusCode = mappedResponse.Any() ? StatusCodes.Status200OK : StatusCodes.Status204NoContent;
        responseModel.Result = pagedResponse;
        return responseModel;
    }
}