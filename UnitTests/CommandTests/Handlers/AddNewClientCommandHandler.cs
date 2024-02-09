using Contracts.Repositories.Client;
using Persistence.Entities;

namespace UnitTests.CommandTests.Handlers;

public class AddNewClientCommandHandler
{
    private readonly IClientRepository _clientRepo;

    public AddNewClientCommandHandler(IClientRepository clientRepo)
    {
        _clientRepo = clientRepo;
    }

    public async Task<string> Handle(Client command, CancellationToken cancellationToken)
    {
        var clientModel = new Client
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber
        };

        var clientGuid = await _clientRepo.AddAsync(clientModel, cancellationToken);
        return clientGuid;
    }
}