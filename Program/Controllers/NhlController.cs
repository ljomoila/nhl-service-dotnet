using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using nhl_service_dotnet.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using nhl_service_dotnet.Exceptions;

namespace nhl_service_dotnet.Controllers;

[ApiController]
[Produces("application/json")]
[Route("/")]
public class NhlController : ControllerBase
{
    private readonly INhlService service;

    public NhlController(INhlService service)
    {
        this.service = service;
    }

    [SwaggerOperation(Summary = "All teams")]
    [HttpGet]
    [Route("teams")]
    public async Task<List<Team>> GetTeams()
    {
        return await service.GetTeams();
    }

    [SwaggerOperation(Summary = "All players")]
    [HttpGet]
    [Route("players")]
    public string GetPlayers()
    {
        throw new NotImplementedException("Get players has not been implemented yet");
    }

    [SwaggerOperation(Summary = "Player stats by id and type")]
    [HttpGet]
    [Route("player/{id}/stats/{type}")]
    public string GetPlayerStats(int id, string type)
    {
        throw new NotImplementedException("Get player stats has not been implemented yet");
    }

    [SwaggerOperation(Summary = "All scheduled games with stats by date")]
    [HttpGet]
    [Route("games/{date}")]
    public string GetGames(string date)
    {
        throw new NotImplementedException("Get games has not been implemented yet");
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
        var exception = exceptionHandlerFeature.Error;
        var status = HttpStatusCode.InternalServerError;

        if (exception is NhlException)
            status = ((NhlException)exception).StatusCode;

        return Problem(
            detail: exception.StackTrace,
            title: exception.Message,
            statusCode: (int?)status
        );
    }
}
