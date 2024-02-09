namespace Contracts.Repositories.Client;

public interface IClientRepository
{
    Task<List<Persistence.Entities.Client>> Get(string keyword, CancellationToken cancellationToken);

    Task<string> AddAsync(Persistence.Entities.Client client, CancellationToken cancellationToken);

    Task<bool> Update(Persistence.Entities.Client client, CancellationToken cancellationToken);

    Task<Persistence.Entities.Client?> GetById(string id, CancellationToken cancellationToken);
    
    Task<Persistence.Entities.Client> GetByEmail(string email, CancellationToken cancellationToken);
    
    Task DeleteById(string clientId, CancellationToken cancellationToken);
}