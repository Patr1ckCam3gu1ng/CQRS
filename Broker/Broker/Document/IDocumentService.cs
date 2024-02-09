namespace Broker.Document;

public interface IDocumentService
{
    Task<string> SyncDocumentsFromExternalSource(string email, CancellationToken cancellationToken);
}