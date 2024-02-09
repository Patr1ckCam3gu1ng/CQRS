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

public class RUpdateClient : IUpdateClient
{
    private readonly IClientProcessorService _clientProcessorService;
    private readonly RetryQueue _retryQueue;
    private readonly ICacheService _cache;
    private readonly IClientRepository _clientRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<RUpdateClient> _log;

    private const string FailedToUpdateMessage = "Failed to update Client details";

    public RUpdateClient(IClientProcessorService clientProcessorService, RetryQueue retryQueue,
        ICacheService cache, IClientRepository clientRepo, IMapper mapper, ILogger<RUpdateClient> log)
    {
        _clientProcessorService = clientProcessorService;
        _retryQueue = retryQueue;
        _cache = cache;
        _clientRepo = clientRepo;
        _mapper = mapper;
        _log = log;
    }

    public async Task<ResponseModel<UpdateClientResponseModel>> Update(UpdateClientRequestModel model, CancellationToken cancellationToken)
    {
        ResponseModel<UpdateClientResponseModel> responseModel = new();

        try
        {
            var existingClient = await _clientRepo.GetById(model.Id, cancellationToken);
            if (existingClient == null)
            {
                responseModel.StatusCode = StatusCodes.Status204NoContent;
                return responseModel;
            }

            var oldEmail = existingClient.Email;

            var mappedClient = _mapper.Map<Persistence.Entities.Client>(model);

            var oldClientData = await _clientRepo.GetById(mappedClient.Id, cancellationToken);

            // Start "2-Phase Commit". Update first the client details.
            var isUpdated = await _clientRepo.Update(mappedClient, cancellationToken);

            // If the email has changed, send an email and sync documents after a client is updated:
            if (oldEmail != model.Email)
            {
                if (isUpdated)
                {
                    const string emailBody = "Hi there - Your email has been changed.";

                    // This becomes the second phase of the commit:
                    var clientProcessed = await _clientProcessorService.Process(model.Email, emailBody, cancellationToken);

                    // If at least one server responds with a FAIL message, the transaction will be aborted.
                    if (clientProcessed.Any())
                    {
                        // Queue in the background to retry again after some seconds
                        QueueRetry(cancellationToken, mappedClient, emailBody);

                        // Revert our changes to the database since our two(2) way commit has failed in one of their processes:
                        if (oldClientData != null) await _clientRepo.Update(oldClientData, cancellationToken);

                        responseModel.StatusCode = StatusCodes.Status400BadRequest;
                        responseModel.Message = JsonSerializer.Serialize(clientProcessed);
                        return responseModel;
                    }

                    _cache.ClearCache();

                    // The transaction will be committed only if 'clientProcessed' replies with 'all' OK.
                    return ReturnOKResponse(mappedClient, responseModel);
                }

                responseModel.StatusCode = StatusCodes.Status400BadRequest;
                responseModel.Message = FailedToUpdateMessage;
                return responseModel;
            }

            if (isUpdated)
            {
                return ReturnOKResponse(mappedClient, responseModel);
            }

            responseModel.StatusCode = StatusCodes.Status400BadRequest;
            responseModel.Message = FailedToUpdateMessage;
            return responseModel;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in RUpdateClient -> Update");
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
                // If success, update the client:
                await clientRepo.Update(mappedClient, cancellationToken);
                
                cache.ClearCache();
            }
            else
            {
                // force the retryQueue to retry again
                throw new Exception("Retrying client process");
            }
        });
    }

    private ResponseModel<UpdateClientResponseModel> ReturnOKResponse(Persistence.Entities.Client mappedClient, ResponseModel<UpdateClientResponseModel> responseModel)
    {
        var mappedResponseModel = _mapper.Map<UpdateClientResponseModel>(mappedClient);

        responseModel.StatusCode = StatusCodes.Status200OK;
        responseModel.Result = mappedResponseModel;
        return responseModel;
    }
}