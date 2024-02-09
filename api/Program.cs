using Broker;
using Contracts;
using Microsoft.OpenApi.Models;
using Persistence;
using Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1", Description = "This API for Carepatron dev" });
    //
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .Build());
});

services.AddMemoryCache();

services.AddControllers();
services.RegisterBrokerServices();

services.RegisterContractServices();
services.RegisterPersistenceServices(configuration);

services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        if (httpReq.Headers.ContainsKey("X-Forwarded-Host"))
        {
            var basePath = "";
            var serverUrl = $"{httpReq.Scheme}://{httpReq.Headers["X-Forwarded-Host"]}/{basePath}";
            swagger.Servers = new List<OpenApiServer> { new() { Url = serverUrl } };
        }
    });
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carepatron.dev.API v1");
    c.RoutePrefix = String.Empty;
});


// seed data
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    dataSeeder.Seed();
}

app.UseRouting();
app.MapControllers();

app.UseHttpsRedirection();
app.UseCors();


// run app
app.Run();