using System.Text.Json;
using AutoMapper;
using Broker.Cache;
using Contracts.Client.Commands.Interfaces;
using Contracts.Client.Commands.Models;
using Contracts.Models;
using Contracts.Repositories.Client;
using Contracts.Services.Interfaces;
using Contracts.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Contracts.Client.Commands;

public class RCreateClient : ICreateClient
{
    private readonly IClientRepository _clientRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<RCreateClient> _log;
    private readonly IClientProcessorService _clientProcessorService;
    private readonly ICacheService _cache;
    private readonly RetryQueue _retryQueue;

    public RCreateClient(IClientRepository clientRepo, IMapper mapper, ILogger<RCreateClient> log,
        IClientProcessorService clientProcessorService, ICacheService cache, RetryQueue retryQueue)
    {
        _clientRepo = clientRepo;
        _mapper = mapper;
        _log = log;
        _clientProcessorService = clientProcessorService;
        _cache = cache;
        _retryQueue = retryQueue;
    }

    public async Task<ResponseModel<CreateClientResponseModel>> Create(CreateClientRequestModel model, CancellationToken cancellationToken)
    {
        ResponseModel<CreateClientResponseModel> responseModel = new();

        try
        {
            var clientByEmail = await _clientRepo.GetByEmail(model.Email, cancellationToken);
            if (clientByEmail != null)
            {
                responseModel.StatusCode = StatusCodes.Status409Conflict;
                responseModel.Message = "Email already exists";
                return responseModel;
            }

            var mappedClient = _mapper.Map<Persistence.Entities.Client>(model);

            var clientId = await _clientRepo.AddAsync(mappedClient, cancellationToken);
            if (string.IsNullOrEmpty(clientId) == false)
            {
                const string emailBody = "Hi there - welcome to my Carepatron portal.";

                // This becomes the second phase of the commit:
                var clientProcessed = await _clientProcessorService.Process(model.Email, emailBody, cancellationToken);

                // If at least one server responds with a FAIL message, the transaction will be aborted.
                if (clientProcessed.Any())
                {
                    // Queue in the background to retry again after some seconds
                    QueueRetry(cancellationToken, mappedClient, emailBody);

                    // Revert our changes to the database since our two(2) way commit has failed in one of their processes:
                    await _clientRepo.DeleteById(mappedClient.Id, cancellationToken);

                    responseModel.StatusCode = StatusCodes.Status400BadRequest;
                    responseModel.Message = JsonSerializer.Serialize(clientProcessed);
                    return responseModel;
                }

                _cache.ClearCache();

                // The transaction will be committed only if 'clientProcessed' replies with 'all' OK.
                responseModel.StatusCode = StatusCodes.Status200OK;
                responseModel.Result = new CreateClientResponseModel()
                {
                    Id = mappedClient.Id
                };

                return responseModel;
            }

            responseModel.StatusCode = StatusCodes.Status400BadRequest;
            responseModel.Message = "Failed to create new Client";
            return responseModel;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in RCreateClient -> Create");
            responseModel.StatusCode = StatusCodes.Status500InternalServerError;
            responseModel.Message = ex.Message;
            return responseModel;
        }

        throw new NotImplementedException();
    }

    private void QueueRetry(CancellationToken cancellationToken, Persistence.Entities.Client mappedClient, string emailBody)
    {
        _retryQueue.Enqueue(async (clientProcessor, clientRepo, cache, token) =>
        {
            var queuedClientProcessed = await clientProcessor.Process(mappedClient.Email, emailBody, token);
            if (queuedClientProcessed.Any() == false)
            {
                // If success, add back again the previously deleted Client:
                await clientRepo.AddAsync(mappedClient, cancellationToken);
                
                cache.ClearCache();
            }
            else
            {
                // force the retryQueue to retry again
                throw new Exception("Retrying client process");
            }
        });
    }
}