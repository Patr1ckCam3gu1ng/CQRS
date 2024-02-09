using Contracts.Repositories.Client;

namespace UnitTests.CommandTests.Handlers;

public class DeleteClientCommandHandler
{
    private readonly IClientRepository _clientRepo;

    public DeleteClientCommandHandler(IClientRepository clientRepo)
    {
        _clientRepo = clientRepo;
    }

    public async Task Handle(string clientId, CancellationToken cancellationToken)
    {
        await _clientRepo.DeleteById(clientId, cancellationToken);
    }
}