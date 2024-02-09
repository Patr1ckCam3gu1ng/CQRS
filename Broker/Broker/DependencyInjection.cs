using Broker.Cache;
using Broker.Document;
using Broker.Email;
using Microsoft.Extensions.DependencyInjection;

namespace Broker;

public static class DependencyInjection
{
    public static void RegisterBrokerServices(this IServiceCollection services)
    {
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IDocumentService, DocumentService>();
        
        services.AddSingleton<ICacheService, MemoryCacheService>();
    }
}