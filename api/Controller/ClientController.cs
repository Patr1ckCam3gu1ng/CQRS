using Contracts.Client.Commands.Interfaces;
using Contracts.Client.Commands.Models;
using Contracts.Client.Queries;
using Contracts.Client.Queries.Models;
using Contracts.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace api.Controller;

[ApiController]
[Route("clients")]
public class ClientController : ControllerBase
{
    private readonly IGetClient _getClient;
    private readonly ICreateClient _createClient;
    private readonly IUpdateClient _updateClient;

    public ClientController(IGetClient getClient, ICreateClient createClient, IUpdateClient updateClient)
    {
        _getClient = getClient;
        _createClient = createClient;
        _updateClient = updateClient;
    }

    /// <summary>
    /// Gets all clients
    /// </summary>
    /// <param name="PaginationFilter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetClientResponseModel>), Status200OK)]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status500InternalServerError)]
    public async Task<ActionResult> Get([FromQuery] PaginationFilter paginationFilter, CancellationToken cancellationToken)
    {
        var response = await _getClient.Get(paginationFilter, cancellationToken);

        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create new client
    /// </summary>
    /// <param name="CreateClientRequest model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateClientRequestModel), Status201Created)]
    [ProducesResponseType(Status400BadRequest)]
    [ProducesResponseType(Status500InternalServerError)]
    [ProducesResponseType(Status409Conflict)]
    public async Task<ActionResult> Create([FromBody] CreateClientRequestModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data. Please check your input.");

        var response = await _createClient.Create(model, cancellationToken);

        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update existing clients
    /// </summary>
    /// <param name="UpdateClientRequest model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [AllowAnonymous]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status500InternalServerError)]
    [ProducesResponseType(typeof(UpdateClientResponseModel), Status200OK)]
    public async Task<ActionResult> Update([FromBody] UpdateClientRequestModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data. Please check your input.");

        var response = await _updateClient.Update(model, cancellationToken);

        return StatusCode(response.StatusCode, response);
    }
}