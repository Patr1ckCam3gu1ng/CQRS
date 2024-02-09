using Contracts.Repositories.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace UnitTests.CommandTests.Handlers;

public class UpdateClientCommandHandler
{
    private readonly IClientRepository _clientRepo;

    public UpdateClientCommandHandler(IClientRepository clientRepo)
    {
        _clientRepo = clientRepo;
    }

    public async Task<bool> Handle(string clientGuidId, CancellationToken cancellationToken)
    {
        var client = await _clientRepo.GetById(clientGuidId, cancellationToken);

        if (client != null)
        {
            client.LastName = "TestLastNameUpdated";

            var response = await _clientRepo.Update(client, cancellationToken);

            return response;
        }

        throw new TestPlatformException("Client to update not found");
    }
}