using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Contracts.Repositories.Client;

public class ClientRepository : IClientRepository
{
    private readonly DataContext _dataContext;

    public ClientRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<string> AddAsync(Persistence.Entities.Client client, CancellationToken cancellationToken)
    {
        client.Id = Guid.NewGuid().ToString();

        await _dataContext.AddAsync(client, cancellationToken);

        var response = await _dataContext.SaveChangesAsync(cancellationToken);

        return response == 1 ? client.Id : null;
    }

    public async Task<List<Persistence.Entities.Client>> Get(string keyword, CancellationToken cancellationToken)
    {
        return await _dataContext.Clients.Where(c =>
                string.IsNullOrEmpty(keyword) || string.Concat(c.FirstName, c.LastName).ToLowerInvariant().Contains(keyword.ToLowerInvariant()))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> Update(Persistence.Entities.Client client, CancellationToken cancellationToken)
    {
        var existingClient = await _dataContext.Clients.FirstOrDefaultAsync(x => x.Id == client.Id, cancellationToken);
        
        if (existingClient != null)
        {
            existingClient.FirstName = client.FirstName;
            existingClient.LastName = client.LastName;
            existingClient.Email = client.Email;
            existingClient.PhoneNumber = client.PhoneNumber;

            var response = await _dataContext.SaveChangesAsync(cancellationToken);

            return response == 1;
        }

        return false;
    }

    public async Task<Persistence.Entities.Client?> GetById(string id, CancellationToken cancellationToken)
    {
        return await _dataContext.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Persistence.Entities.Client> GetByEmail(string email, CancellationToken cancellationToken)
    {
        return await _dataContext.Clients.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task DeleteById(string clientId, CancellationToken cancellationToken)
    {
        var client = await GetById(clientId, cancellationToken);
        if (client != null)
        {
            _dataContext.Clients.Remove(client);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}