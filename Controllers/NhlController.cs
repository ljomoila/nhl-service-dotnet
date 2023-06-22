using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using nhl_service_dotnet.Services;

namespace nhl_service_dotnet.Controllers;

[ApiController]
[Route("/")]
public class NhlController : ControllerBase
{
    private readonly INhlService _service;

    public NhlController(INhlService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("teams")]
    public async Task<Team[]> GetTeams()
    {
        return await _service.GetTeams();
    }

    [HttpGet]
    [Route("players")]
    public string GetPlayers()
    {
        throw new NotImplementedException("Get players has not been implemented yet");
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        return Problem(
            detail: exceptionHandlerFeature.Error.StackTrace,
            title: exceptionHandlerFeature.Error.Message
        );
    }
}
