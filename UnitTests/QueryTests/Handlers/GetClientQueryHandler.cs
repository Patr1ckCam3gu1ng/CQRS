using Contracts.Repositories.Client;
using Persistence.Entities;

namespace UnitTests.QueryTests.Handlers;

public class GetClientQueryHandler
{
    private readonly IClientRepository _clientRepo;

    public GetClientQueryHandler(IClientRepository clientRepo)
    {
        _clientRepo = clientRepo;
    }
    
    public async Task<Client?> Handle(string clientGuidId, CancellationToken cancellationToken)
    {
        return await _clientRepo.GetById(clientGuidId, cancellationToken);
    }
}