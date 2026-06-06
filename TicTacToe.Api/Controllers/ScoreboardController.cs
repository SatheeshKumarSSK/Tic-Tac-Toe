using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Helpers;
using TicTacToe.Api.Models.Game;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers
{
    [Route("api/scoreboard"), ApiController]
    public class ScoreboardController : ControllerBase
    {
        private readonly IGameService _gameService;

        public ScoreboardController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetScoreboard()
        {
            var result = await _gameService.GetScoreboard();
            return Ok(new HttpResponse<ScoreboardResponse>(StatusCodes.Status200OK, "Fetched Successfully", result));
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetScoreboard()
        {
            var result = await _gameService.ResetScoreboard();
            return Ok(new HttpResponse<ScoreboardResponse>(StatusCodes.Status200OK, "Success", result));
        }
    }
}
