using AutoMapper;
using Broker.Cache;
using Contracts.Client.Queries;
using Contracts.Client.Queries.Models;
using Contracts.Filters;
using Contracts.Models;
using Contracts.Repositories.Client;
using Contracts.Wrappers;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Entities;
using UnitTests.Common;

namespace UnitTests.ServiceTests;

[TestClass]
public class ClientServiceQueryTests
{
    private Mock<IMapper> _mockMapper;
    private Mock<IClientRepository> _mockClientRepository;
    private Mock<ILogger<RGetClient>> _mockLogger;
    private Mock<ICacheService> _mockCacheService;
    private RGetClient _clientService;

    [TestInitialize]
    public void Initialize()
    {
        _mockMapper = new Mock<IMapper>();
        _mockClientRepository = new Mock<IClientRepository>();
        _mockLogger = new Mock<ILogger<RGetClient>>();
        _mockCacheService = new Mock<ICacheService>();

        _clientService = new RGetClient(_mockMapper.Object, _mockClientRepository.Object,
            _mockLogger.Object, _mockCacheService.Object);
    }

    [TestMethod]
    public async Task ShouldRGetClient_WhenCalledWithValidPagination()
    {
        var fakeClients = new List<Client> { TestData.ClientTestData };
        var fakeClientResponseModels = new List<GetClientResponseModel>()
        {
            new()
            {
                Email = TestData.ClientTestData.Email
            }
        };

        _mockClientRepository.Setup(repo => repo.Get(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeClients);

        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<GetClientResponseModel>>(fakeClients))
            .Returns(fakeClientResponseModels);

        var paginationFilter = new PaginationFilter()
        {
            q = "",
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var result = await _clientService.Get(paginationFilter, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StatusCode == 200);
        Assert.IsInstanceOfType(result, typeof(ResponseModel<PagedResponse<IEnumerable<GetClientResponseModel>>>));
        _mockClientRepository.Verify(repo => repo.Get(string.Empty, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}