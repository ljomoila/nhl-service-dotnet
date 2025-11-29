using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using nhl_service_dotnet.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using nhl_service_dotnet.Exceptions;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Controllers;

[ApiController]
[Produces("application/json")]
[Route("/")]
public class NhlController : ControllerBase
{
    private readonly ITeamService teamService;
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;
    private readonly ITeamRosterService teamRosterService;

    [ActivatorUtilitiesConstructor]
    public NhlController(ITeamService teamService, IPlayerService playerService, IGameService gameService, ITeamRosterService teamRosterService)
    {
        this.teamService = teamService;
        this.playerService = playerService;
        this.gameService = gameService;
        this.teamRosterService = teamRosterService;
    }

    [SwaggerOperation(Summary = "Get teams")]
    [HttpGet]
    [Route("teams")]
    public async Task<List<Team>> GetTeams()
    {
        return await teamService.GetTeams();
    }

    [SwaggerOperation(Summary = "Get teams with rosters")]
    [HttpGet]
    [Route("teams/rosters")]
    public async Task<List<TeamRoster>> GetTeamsWithRosters()
    {
        return await teamRosterService.GetTeamsWithRosters();
    }

    [SwaggerOperation(Summary = "Refresh teams and rosters from NHL API into database")]
    [HttpPost]
    [Route("teams/rosters/refresh")]
    public async Task<IActionResult> RefreshTeamsWithRosters()
    {
        await teamRosterService.RefreshTeamsAndRosters();
        return Ok(new { message = "Teams and rosters refreshed" });
    }

    [SwaggerOperation(Summary = "Get player")]
    [HttpGet]
    [Route("players/{id}")]
    public async Task<Player> GetPlayer(int id)
    {
        return await playerService.GetPlayer(id);
    }

    [SwaggerOperation(Summary = "Get player stats by type")]
    [HttpGet]
    [Route("players/{id}/stats/{type}")]
    public string GetPlayerStats(int id, string type)
    {
        throw new NotImplementedException("Get player stats has not been implemented yet");
    }

    [SwaggerOperation(Summary = " Get scheduled games and stats by date")]
    [HttpGet]
    [Route("games/{date}")]
    public async Task<List<Game>> GetGames(string date)
    {
        return await gameService.GetGames(date);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
        var exception = exceptionHandlerFeature.Error;
        var status = HttpStatusCode.InternalServerError;

        if (exception is NhlException)
        {
            status = ((NhlException)exception).StatusCode;
        }

        return Problem(
            detail: exception.StackTrace,
            title: exception.Message,
            statusCode: (int?)status
        );
    }
}
