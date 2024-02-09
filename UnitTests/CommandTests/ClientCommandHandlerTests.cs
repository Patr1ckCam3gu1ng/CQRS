using Contracts.Repositories.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Context;
using UnitTests.CommandTests.Handlers;
using UnitTests.Common;
using UnitTests.QueryTests.Handlers;

namespace UnitTests.CommandTests;

[TestClass]
public class ClientCommandHandlerTests
{
    private DataContext _dbContext;
    private ClientRepository _clientRepository;

    private AddNewClientCommandHandler _addNewClientHandler;
    private UpdateClientCommandHandler _updateClientHandler;
    private DeleteClientCommandHandler _deleteClientHandler;

    private GetClientQueryHandler _getClientHandler;
    private readonly string _databaseName;

    public ClientCommandHandlerTests()
    {
        var config  = new ConfigurationBuilder()
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
        _updateClientHandler = new UpdateClientCommandHandler(_clientRepository);
        _deleteClientHandler = new DeleteClientCommandHandler(_clientRepository);

        _getClientHandler = new GetClientQueryHandler(_clientRepository);
    }

    [TestMethod]
    public async Task ShouldCreateClient()
    {
        // Act
        var clientGuidId = await _addNewClientHandler.Handle(TestData.ClientTestData, CancellationToken.None);

        Assert.IsFalse(string.IsNullOrEmpty(clientGuidId));
    }

    [TestMethod]
    public async Task ShouldUpdateClient()
    {
        var client = await _addNewClientHandler.Handle(TestData.ClientTestData, CancellationToken.None);

        // Act
        var isClientUpdated = await _updateClientHandler.Handle(client, CancellationToken.None);

        Assert.IsTrue(isClientUpdated);
    }

    [TestMethod]
    public async Task ShouldDeleteUser()
    {
        var newClientGuid = await _addNewClientHandler.Handle(TestData.ClientTestData, CancellationToken.None);

        if (string.IsNullOrEmpty(newClientGuid) == false)
        {
            // Act
            await _deleteClientHandler.Handle(newClientGuid, CancellationToken.None);

            // Check if the client was indeed deleted:
            var secondCheckClient = await _getClientHandler.Handle(newClientGuid, CancellationToken.None);

            Assert.IsNull(secondCheckClient);
        }
        else
        {
            Assert.Fail("No client record has been created");
        }
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}