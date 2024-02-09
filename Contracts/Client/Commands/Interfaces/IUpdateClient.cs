using System.Collections.Concurrent;
using Contracts.Client.Commands.Models;
using Contracts.Models;

namespace Contracts.Client.Commands.Interfaces;

public interface IUpdateClient
{
    Task<ResponseModel<UpdateClientResponseModel>> Update(UpdateClientRequestModel model, CancellationToken cancellationToken);
}