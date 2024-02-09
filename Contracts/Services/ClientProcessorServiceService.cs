using Broker.Document;
using Broker.Email;
using Contracts.Services.Interfaces;

namespace Contracts.Services;

public class ClientProcessorServiceService : IClientProcessorService
{
    private readonly IEmailService _email;
    private readonly IDocumentService _document;

    public ClientProcessorServiceService(IEmailService email, IDocumentService document)
    {
        _email = email;
        _document = document;
    }

    public async Task<List<string>> Process(string emailAddress, string emailBody, CancellationToken cancellationToken)
    {
        var brokerServiceResponses = new List<string>();

        var documentServiceResponse = await ProcessDocument(emailAddress, cancellationToken);
        if (!string.IsNullOrEmpty(documentServiceResponse))
        {
            // Log the error to be returned as a response:
            brokerServiceResponses.Add(documentServiceResponse);
        }

        // Proceed on emailing the client only if the document sync has succeeded:
        if (brokerServiceResponses.Any() == false)
        {
            var emailServiceResponse = await ProcessEmailer(emailAddress, emailBody, cancellationToken);
            if (!string.IsNullOrEmpty(emailServiceResponse))
            {
                // Log the error to be returned as a response:
                brokerServiceResponses.Add(emailServiceResponse);
            }
        }

        return brokerServiceResponses;
    }

    private async Task<string> ProcessEmailer(string emailAddress, string emailBody, CancellationToken cancellationToken)
    {
        var response = await _email.Send(emailAddress, emailBody, cancellationToken);
        return response;
    }

    private async Task<string> ProcessDocument(string emailAddress, CancellationToken cancellationToken)
    {
        var response = await _document.SyncDocumentsFromExternalSource(emailAddress, cancellationToken);
        return response;
    }
}