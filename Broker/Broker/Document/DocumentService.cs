using Microsoft.Extensions.Logging;
using Shared.Utilities;

namespace Broker.Document;

public class DocumentService : IDocumentService
{
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ILogger<DocumentService> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> SyncDocumentsFromExternalSource(string email, CancellationToken cancellationToken)
    {
        try
        {
            // simulates random errors that occur with external services
            // leave this to emulate real life
            ChaosUtility.RollTheDice();

            // this simulates sending an email
            // leave this delay as 10s to emulate real life
            await Task.Delay(10000, cancellationToken);

            return string.Empty;
        }
        catch (Exception ex)
        {
            var errMessage = $"There was a problem syncing the document/s from external source. Failed with - {ex}";
            _logger.LogError(errMessage);
            return errMessage;
        }
    }
}