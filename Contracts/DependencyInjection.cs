using Contracts.Client.Commands;
using Contracts.Client.Commands.Interfaces;
using Contracts.Client.Queries;
using Contracts.Repositories.Client;
using Contracts.Services;
using Contracts.Services.Interfaces;
using Contracts.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

public static class DependencyInjection
{
    public static void RegisterContractServices(this IServiceCollection services)
    {
        #region Services

        services.AddScoped<IGetClient, RGetClient>();
        services.AddScoped<ICreateClient, RCreateClient>();
        services.AddScoped<IUpdateClient, RUpdateClient>();
        services.AddScoped<IClientProcessorService, ClientProcessorServiceService>();

        services.AddSingleton<RetryQueue>();
        services.AddHostedService<RetryBackgroundService>();
        
        #endregion

        #region Repositories

        services.AddScoped<IClientRepository, ClientRepository>();

        #endregion
    }
}