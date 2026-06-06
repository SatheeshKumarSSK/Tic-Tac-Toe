using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Helpers;
using TicTacToe.Api.Models.Game;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers
{
    [Route("api/games"), ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            var result = await _gameService.CreateGame(request);
            return Ok(new HttpResponse<GameStateResponse>(StatusCodes.Status201Created, "Created Successfully", result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var result = await _gameService.GetGame(id);
            return Ok(new HttpResponse<GameStateResponse>(StatusCodes.Status200OK, "Fetched Successfully", result));
        }

        [HttpPost("{id}/moves")]
        public async Task<IActionResult> MakeMove(int id, [FromBody] MoveRequest request)
        {
            var result = await _gameService.MakeMove(id, request);
            return Ok(new HttpResponse<GameStateResponse>(StatusCodes.Status200OK, "Success", result));
        }

        [HttpPost("{id}/undo")]
        public async Task<IActionResult> Undo(int id)
        {
            var result = await _gameService.Undo(id);
            return Ok(new HttpResponse<GameStateResponse>(StatusCodes.Status200OK, "Success", result));
        }

        [HttpPost("{id}/reset")]
        public async Task<IActionResult> ResetGame(int id)
        {
            var result = await _gameService.ResetGame(id);
            return Ok(new HttpResponse<GameStateResponse>(StatusCodes.Status200OK, "Success", result));
        }
    }
}
