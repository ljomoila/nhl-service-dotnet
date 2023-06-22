using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using nhl_service_dotnet.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace nhl_service_dotnet.Controllers;

[ApiController]
[Produces("application/json")]
[Route("/")]
public class NhlController : ControllerBase
{
    private readonly INhlService _service;

    public NhlController(INhlService service)
    {
        _service = service;
    }

    [SwaggerOperation(Summary = "All teams")]
    [HttpGet]
    [Route("teams")]
    public async Task<Team[]> GetTeams()
    {
        return await _service.GetTeams();
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
        var status = 500;

        if (exception is HttpRequestException)
            status = (int)((HttpRequestException)exception).StatusCode;

        return Problem(detail: exception.StackTrace, title: exception.Message, statusCode: status);
    }
}
