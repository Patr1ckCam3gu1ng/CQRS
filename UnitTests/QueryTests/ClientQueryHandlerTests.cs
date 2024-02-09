using Contracts.Repositories.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Context;
using Persistence.Entities;
using UnitTests.CommandTests.Handlers;
using UnitTests.Common;
using UnitTests.QueryTests.Handlers;

namespace UnitTests.QueryTests;

[TestClass]
public class ClientQueryHandlerTests
{
    private DataContext _dbContext;
    private ClientRepository _clientRepository;

    private GetClientQueryHandler _getClientHandler;
    private AddNewClientCommandHandler _addNewClientHandler;
    private readonly string _databaseName;

    public ClientQueryHandlerTests()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path to the current directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _databaseName = config["Database:Name"] ?? throw new NullReferenceException();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _dbContext = new DataContext(new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

        _clientRepository = new ClientRepository(_dbContext);

        _addNewClientHandler = new AddNewClientCommandHandler(_clientRepository);
        _getClientHandler = new GetClientQueryHandler(_clientRepository);
    }

    [TestMethod]
    public async Task ShouldGetClient()
    {
        var newClientGuid = await _addNewClientHandler.Handle(TestData.ClientTestData, CancellationToken.None);

        // Act
        var client = await _getClientHandler.Handle(newClientGuid, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType(client, typeof(Client), "The result should be of type Client.");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}