using Microsoft.Extensions.Logging;
using Shared.Utilities;

namespace Broker.Email;

public class EmailService : IEmailService
{
    private readonly ILogger _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<string> Send(string email, string message, CancellationToken cancellationToken)
    {
        try
        {
            // simulates random errors that occur with external services
            // leave this to emulate real life
            ChaosUtility.RollTheDice();

            // simulates sending an email
            // leave this delay as 10s to emulate real life
            await Task.Delay(10000, cancellationToken);

            return string.Empty;
        }
        catch (Exception ex)
        {
            var errMessage = $"There was a problem sending the email. Failed with - {ex}";
            _logger.LogError(errMessage);

            return errMessage;
        }
    }
}