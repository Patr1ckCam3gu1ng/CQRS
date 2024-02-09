namespace Contracts.Services.Interfaces;

public interface IClientProcessorService
{
    Task<List<string>> Process(string emailAddress, string emailBody, CancellationToken cancellationToken);
}