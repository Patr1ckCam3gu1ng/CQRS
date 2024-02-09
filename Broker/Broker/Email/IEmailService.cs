namespace Broker.Email;

public interface IEmailService
{
    Task<string> Send(string email, string message, CancellationToken cancellationToken);
}