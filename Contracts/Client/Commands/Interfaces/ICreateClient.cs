using Contracts.Client.Commands.Models;
using Contracts.Models;

namespace Contracts.Client.Commands.Interfaces;

public interface ICreateClient
{
    Task<ResponseModel<CreateClientResponseModel>> Create(CreateClientRequestModel model, CancellationToken cancellationToken);
}