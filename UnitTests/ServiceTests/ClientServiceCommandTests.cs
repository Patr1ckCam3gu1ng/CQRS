using AutoMapper;
using Broker.Cache;
using Contracts.Client.Commands;
using Contracts.Client.Commands.Models;
using Contracts.Models;
using Contracts.Repositories.Client;
using Contracts.Services.Interfaces;
using Contracts.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Entities;
using UnitTests.Common;

namespace UnitTests.ServiceTests;

[TestClass]
public class ClientServiceCommandTests
{
    private Mock<IMapper> _mockMapper;
    private Mock<IClientRepository> _mockClientRepository;
    private Mock<IClientProcessorService> _mockClientProcessor;
    private Mock<ILogger<RCreateClient>> _mockCreateClientLogger;
    private Mock<ICacheService> _mockCacheService;
    private RCreateClient _createClientService;
    private RUpdateClient _updateClientService;
    private Mock<ILogger<RUpdateClient>> _mockUpdateClientLogger;
    private Mock<RetryQueue> _mockRetryQueue;

    [TestInitialize]
    public void Initialize()
    {
        _mockMapper = new Mock<IMapper>();
        _mockClientRepository = new Mock<IClientRepository>();
        _mockClientProcessor = new Mock<IClientProcessorService>();
        _mockClientProcessor = new Mock<IClientProcessorService>();
        _mockRetryQueue = new Mock<RetryQueue>();
        _mockCreateClientLogger = new Mock<ILogger<RCreateClient>>();
        _mockUpdateClientLogger = new Mock<ILogger<RUpdateClient>>();
        _mockCacheService = new Mock<ICacheService>();

        _createClientService = new RCreateClient(_mockClientRepository.Object, _mockMapper.Object,
            _mockCreateClientLogger.Object, _mockClientProcessor.Object, _mockCacheService.Object,
            _mockRetryQueue.Object);

        _updateClientService = new RUpdateClient(_mockClientProcessor.Object, _mockRetryQueue.Object, _mockCacheService.Object, _mockClientRepository.Object,
            _mockMapper.Object, _mockUpdateClientLogger.Object);
    }

    [TestMethod]
    public async Task Create_ShouldReturnStatusCode200()
    {
        var fakeClient = new Client();
        fakeClient = TestData.ClientTestData;

        var fakeCreateClientRequestModel = new CreateClientRequestModel()
        {
            Email = TestData.ClientTestData.Email,
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        _mockClientRepository.Setup(repo => repo.GetByEmail(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.ClientTestData);

        _mockMapper.Setup(mapper => mapper.Map<Client>(fakeCreateClientRequestModel))
            .Returns(fakeClient);

        _mockClientRepository.Setup(repo => repo.AddAsync(fakeClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        _mockClientProcessor.Setup(proc => proc.Process(fakeCreateClientRequestModel.Email, It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _createClientService.Create(fakeCreateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 200);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<CreateClientResponseModel>));
        _mockClientRepository.Verify(repo => repo.GetByEmail(TestData.ClientTestData.Email, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockClientRepository.Verify(repo => repo.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockClientProcessor.Verify(repo => repo.Process(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task Create_ShouldReturnStatus409Conflict()
    {
        var fakeCreateClientRequestModel = new CreateClientRequestModel()
        {
            Email = TestData.ClientTestData.Email,
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        _mockClientRepository.Setup(repo => repo.GetByEmail(TestData.ClientTestData.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.ClientTestData);

        // Act
        var result = await _createClientService.Create(fakeCreateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 409);
        Assert.IsNotNull(result.Message);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<CreateClientResponseModel>));
        _mockClientRepository.Verify(repo => repo.GetByEmail(TestData.ClientTestData.Email, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task Create_ShouldReturnStatus400BadRequest()
    {
        var fakeClient = new Client();
        fakeClient = TestData.ClientTestData;

        var fakeCreateClientRequestModel = new CreateClientRequestModel()
        {
            Email = TestData.ClientTestData.Email,
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        _mockClientRepository.Setup(repo => repo.GetByEmail(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.ClientTestData);

        _mockMapper.Setup(mapper => mapper.Map<Client>(fakeCreateClientRequestModel))
            .Returns(fakeClient);

        _mockClientRepository.Setup(repo => repo.AddAsync(fakeClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        // Make client processor throw an error:
        _mockClientProcessor.Setup(proc => proc.Process(fakeCreateClientRequestModel.Email, It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new List<string>()
            {
                "Error badly occured",
                "Yikes! FUBAR encountered"
            });


        // Act
        var result = await _createClientService.Create(fakeCreateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 400);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<CreateClientResponseModel>));

        _mockClientRepository.Verify(repo => repo.GetByEmail(TestData.ClientTestData.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockClientRepository.Verify(repo => repo.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockClientRepository.Verify(repo => repo.DeleteById(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        _mockClientProcessor.Verify(repo => repo.Process(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Update_ShouldReturnStatus204NoContent()
    {
        var fakeUpdateClientRequestModel = new UpdateClientRequestModel()
        {
            Id = TestData.ClientTestData.Id,
            Email = TestData.ClientTestData.Email,
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        // Act
        var result = await _updateClientService.Update(fakeUpdateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 204);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<UpdateClientResponseModel>));
        _mockClientRepository.Verify(repo => repo.GetById(TestData.ClientTestData.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Update_ShouldReturnOKResponse()
    {
        var fakeUpdateClientRequestModel = new UpdateClientRequestModel()
        {
            Id = TestData.ClientTestData.Id,
            Email = TestData.ClientTestData.Email,
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        _mockMapper.Setup(mapper => mapper.Map<Client>(fakeUpdateClientRequestModel))
            .Returns(TestData.ClientTestData);

        // oldClientData:
        _mockClientRepository.Setup(repo => repo.GetById(TestData.ClientTestData.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.ClientTestData);

        _mockClientRepository.Setup(repo => repo.Update(TestData.ClientTestData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _updateClientService.Update(fakeUpdateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 200);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<UpdateClientResponseModel>));
        _mockClientRepository.Verify(repo => repo.GetById(TestData.ClientTestData.Id, It.IsAny<CancellationToken>()), Times.AtLeast(2));
        _mockClientRepository.Verify(repo => repo.Update(TestData.ClientTestData, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Update_ShouldReturnOKResponse_WithClientProcessor()
    {
        var fakeUpdateClientRequestModel = new UpdateClientRequestModel()
        {
            Id = TestData.ClientTestData.Id,
            Email = "shouldUpdateNewEmail@yahoo.com",
            FirstName = TestData.ClientTestData.FirstName,
            LastName = TestData.ClientTestData.LastName,
            PhoneNumber = TestData.ClientTestData.PhoneNumber
        };

        _mockMapper.Setup(mapper => mapper.Map<Client>(fakeUpdateClientRequestModel))
            .Returns(TestData.ClientTestData);

        // oldClientData:
        _mockClientRepository.Setup(repo => repo.GetById(TestData.ClientTestData.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.ClientTestData);

        _mockClientRepository.Setup(repo => repo.Update(TestData.ClientTestData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // No error
        _mockClientProcessor.Setup(proc => proc.Process(fakeUpdateClientRequestModel.Email, It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _updateClientService.Update(fakeUpdateClientRequestModel, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 200);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<UpdateClientResponseModel>));
        _mockClientRepository.Verify(repo => repo.GetById(TestData.ClientTestData.Id, It.IsAny<CancellationToken>()), Times.AtLeast(2));
        _mockClientRepository.Verify(repo => repo.Update(TestData.ClientTestData, It.IsAny<CancellationToken>()), Times.Once);

        _mockClientProcessor.Verify(repo => repo.Process(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}