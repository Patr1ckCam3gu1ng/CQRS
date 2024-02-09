using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;

namespace Persistence;

public static class DependencyInjection
{
    public static void RegisterPersistenceServices(this IServiceCollection services, ConfigurationManager config)
    {
        var databaseName = config["Database:Name"] ?? throw new NullReferenceException();

        services.AddScoped<DataSeeder>();
        services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase(databaseName));
    }
}