using TicTacToe.Api.Helpers;
using TicTacToe.Api.Models.Entities;
using TicTacToe.Api.Models.Game;
using TicTacToe.Api.Repositories;

namespace TicTacToe.Api.Services
{
    public interface IGameService
    {
        Task<GameStateResponse> CreateGame(CreateGameRequest request);
        Task<GameStateResponse> GetGame(int gameId);
        Task<GameStateResponse> MakeMove(int gameId, MoveRequest request);
        Task<GameStateResponse> Undo(int gameId);
        Task<GameStateResponse> ResetGame(int gameId);
        Task<ScoreboardResponse> GetScoreboard();
        Task<ScoreboardResponse> ResetScoreboard();
    }

    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly string[] _validModes = ["TwoPlayer", "Computer"];
        private readonly string[] _validPlayers = ["X", "O"];

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        #region Game
        public async Task<GameStateResponse> CreateGame(CreateGameRequest request)
        {
            var mode = string.IsNullOrWhiteSpace(request.GameMode) ? "TwoPlayer" : request.GameMode;

            if (!_validModes.Contains(mode))
                throw new PZException(StatusCodes.Status400BadRequest, "Invalid Game Mode", mode);

            var game = new GameSession()
            {
                GameMode = mode,
                CurrentPlayer = "X",
                GameStatus = "InProgress",
                Winner = null,
                WinningCells = "",
                ScoreboardUpdated = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            game = await _gameRepository.CreateGame(game);
            return await BuildResponse(game.GameId);
        }

        public async Task<GameStateResponse> GetGame(int gameId)
        {
            var game = await GetGameOrThrow(gameId);
            return await BuildResponse(game.GameId);
        }

        public async Task<GameStateResponse> MakeMove(int gameId, MoveRequest request)
        {
            var game = await GetGameOrThrow(gameId);

            if (request.GameId != gameId)
                throw new PZException(StatusCodes.Status400BadRequest, "Game Id Does Not Match", request.GameId);

            ValidateMove(game, request);

            await AddMove(game, request.Player, request.Row, request.Column);
            game = await GetGameOrThrow(gameId);

            var board = BuildBoard(game.Moves);
            ApplyGameResult(game, board);

            if (game.GameStatus == "InProgress")
            {
                game.CurrentPlayer = game.GameMode == "Computer" ? "O" : NextPlayer(request.Player);
                game.DateModified = DateTime.UtcNow;
                await _gameRepository.UpdateGame(game);
            }
            else
            {
                await CompleteGame(game);
                return await BuildResponse(game.GameId);
            }

            if (game.GameMode == "Computer" && game.CurrentPlayer == "O")
            {
                game = await GetGameOrThrow(gameId);
                board = BuildBoard(game.Moves);
                var computerMove = GetComputerMove(board);
                await AddMove(game, "O", computerMove.Row, computerMove.Column);
                game = await GetGameOrThrow(gameId);
                board = BuildBoard(game.Moves);
                ApplyGameResult(game, board);

                if (game.GameStatus == "InProgress")
                    game.CurrentPlayer = "X";

                if (game.GameStatus != "InProgress")
                    await CompleteGame(game);
                else
                {
                    game.DateModified = DateTime.UtcNow;
                    await _gameRepository.UpdateGame(game);
                }
            }

            return await BuildResponse(gameId);
        }

        public async Task<GameStateResponse> Undo(int gameId)
        {
            var game = await GetGameOrThrow(gameId);

            if (game.GameStatus != "InProgress")
                throw new PZException(StatusCodes.Status400BadRequest, "Undo Disabled After Completion", "");

            var moves = game.Moves.OrderBy(m => m.MoveNumber).ToList();

            if (!moves.Any())
                return await BuildResponse(game.GameId);

            var movesToRemove = new List<GameMove>();

            if (game.GameMode == "Computer")
            {
                var lastMove = moves.Last();
                movesToRemove.Add(lastMove);

                if (lastMove.Player == "O")
                {
                    var previousHumanMove = moves.LastOrDefault(m => m.MoveNumber < lastMove.MoveNumber && m.Player == "X");
                    if (previousHumanMove != null)
                        movesToRemove.Add(previousHumanMove);
                }
            }
            else
            {
                movesToRemove.Add(moves.Last());
            }

            await _gameRepository.DeleteMoves(movesToRemove);
            game = await GetGameOrThrow(gameId);
            game.CurrentPlayer = game.GameMode == "Computer" ? "X" : movesToRemove.OrderBy(m => m.MoveNumber).First().Player;
            game.GameStatus = "InProgress";
            game.Winner = null;
            game.WinningCells = "";
            game.ScoreboardUpdated = false;
            game.DateModified = DateTime.UtcNow;
            await _gameRepository.UpdateGame(game);

            return await BuildResponse(game.GameId);
        }

        public async Task<GameStateResponse> ResetGame(int gameId)
        {
            var game = await GetGameOrThrow(gameId);
            var moves = game.Moves.ToList();

            if (moves.Any())
                await _gameRepository.DeleteMoves(moves);

            game.CurrentPlayer = "X";
            game.GameStatus = "InProgress";
            game.Winner = null;
            game.WinningCells = "";
            game.ScoreboardUpdated = false;
            game.DateModified = DateTime.UtcNow;
            await _gameRepository.UpdateGame(game);

            return await BuildResponse(game.GameId);
        }
        #endregion

        #region Scoreboard
        public async Task<ScoreboardResponse> GetScoreboard()
        {
            var scoreboard = await _gameRepository.GetScoreboard();
            return MapScoreboard(scoreboard);
        }

        public async Task<ScoreboardResponse> ResetScoreboard()
        {
            await _gameRepository.ResetScoreboard();
            var scoreboard = await _gameRepository.GetScoreboard();
            return MapScoreboard(scoreboard);
        }
        #endregion

        #region Common Methods
        private async Task<GameSession> GetGameOrThrow(int gameId)
        {
            var game = await _gameRepository.GetGameWithMoves(gameId);

            if (game == null)
                throw new PZException(StatusCodes.Status404NotFound, "Game Not Found", gameId);

            game.Moves = game.Moves.OrderBy(m => m.MoveNumber).ToList();
            return game;
        }

        private void ValidateMove(GameSession game, MoveRequest request)
        {
            if (game.GameStatus != "InProgress")
                throw new PZException(StatusCodes.Status400BadRequest, "Move After Game Completion Is Not Allowed", "");

            if (!_validPlayers.Contains(request.Player))
                throw new PZException(StatusCodes.Status400BadRequest, "Invalid Player", request.Player);

            if (request.Player != game.CurrentPlayer)
                throw new PZException(StatusCodes.Status400BadRequest, "Wrong Player Turn", request.Player);

            if (game.GameMode == "Computer" && request.Player != "X")
                throw new PZException(StatusCodes.Status400BadRequest, "Computer Controls Player O", request.Player);

            if (request.Row < 0 || request.Row > 2 || request.Column < 0 || request.Column > 2)
                throw new PZException(StatusCodes.Status400BadRequest, "Move Outside The Board", request);

            var board = BuildBoard(game.Moves);

            if (!string.IsNullOrWhiteSpace(board[request.Row, request.Column]))
                throw new PZException(StatusCodes.Status400BadRequest, "Cell Already Occupied", request);
        }

        private async Task AddMove(GameSession game, string player, int row, int column)
        {
            var move = new GameMove()
            {
                GameId = game.GameId,
                MoveNumber = game.Moves.Count + 1,
                Player = player,
                Row = row,
                Column = column,
                DateCreated = DateTime.UtcNow
            };

            await _gameRepository.AddMove(move);
        }

        private string[,] BuildBoard(List<GameMove> moves)
        {
            var board = new string[3, 3];

            foreach (var move in moves.OrderBy(m => m.MoveNumber))
            {
                if (move.Row >= 0 && move.Row <= 2 && move.Column >= 0 && move.Column <= 2)
                    board[move.Row, move.Column] = move.Player;
            }

            return board;
        }

        private void ApplyGameResult(GameSession game, string[,] board)
        {
            var winningCells = GetWinningCells(board);

            if (winningCells.Any())
            {
                game.GameStatus = "Won";
                game.Winner = board[winningCells[0].Row, winningCells[0].Column];
                game.WinningCells = SerializeCells(winningCells);
                game.CurrentPlayer = game.Winner;
            }
            else if (game.Moves.Count == 9)
            {
                game.GameStatus = "Draw";
                game.Winner = null;
                game.WinningCells = "";
            }
            else
            {
                game.GameStatus = "InProgress";
                game.Winner = null;
                game.WinningCells = "";
            }
        }

        private async Task CompleteGame(GameSession game)
        {
            if (!game.ScoreboardUpdated)
            {
                var scoreboard = await _gameRepository.GetScoreboard();

                if (game.GameStatus == "Won" && game.Winner == "X")
                    scoreboard.XWins++;
                else if (game.GameStatus == "Won" && game.Winner == "O")
                    scoreboard.OWins++;
                else if (game.GameStatus == "Draw")
                    scoreboard.Draws++;

                await _gameRepository.UpdateScoreboard(scoreboard);
                game.ScoreboardUpdated = true;
            }

            game.DateModified = DateTime.UtcNow;
            await _gameRepository.UpdateGame(game);
        }

        private List<CellModel> GetWinningCells(string[,] board)
        {
            var lines = new List<List<CellModel>>()
            {
                new List<CellModel>() { new CellModel { Row = 0, Column = 0 }, new CellModel { Row = 0, Column = 1 }, new CellModel { Row = 0, Column = 2 } },
                new List<CellModel>() { new CellModel { Row = 1, Column = 0 }, new CellModel { Row = 1, Column = 1 }, new CellModel { Row = 1, Column = 2 } },
                new List<CellModel>() { new CellModel { Row = 2, Column = 0 }, new CellModel { Row = 2, Column = 1 }, new CellModel { Row = 2, Column = 2 } },
                new List<CellModel>() { new CellModel { Row = 0, Column = 0 }, new CellModel { Row = 1, Column = 0 }, new CellModel { Row = 2, Column = 0 } },
                new List<CellModel>() { new CellModel { Row = 0, Column = 1 }, new CellModel { Row = 1, Column = 1 }, new CellModel { Row = 2, Column = 1 } },
                new List<CellModel>() { new CellModel { Row = 0, Column = 2 }, new CellModel { Row = 1, Column = 2 }, new CellModel { Row = 2, Column = 2 } },
                new List<CellModel>() { new CellModel { Row = 0, Column = 0 }, new CellModel { Row = 1, Column = 1 }, new CellModel { Row = 2, Column = 2 } },
                new List<CellModel>() { new CellModel { Row = 0, Column = 2 }, new CellModel { Row = 1, Column = 1 }, new CellModel { Row = 2, Column = 0 } }
            };

            foreach (var line in lines)
            {
                var first = board[line[0].Row, line[0].Column];

                if (!string.IsNullOrWhiteSpace(first) &&
                    board[line[1].Row, line[1].Column] == first &&
                    board[line[2].Row, line[2].Column] == first)
                    return line;
            }

            return new List<CellModel>();
        }

        private CellModel GetComputerMove(string[,] board)
        {
            var winningMove = FindBestMove(board, "O");
            if (winningMove != null)
                return winningMove;

            var blockingMove = FindBestMove(board, "X");
            if (blockingMove != null)
                return blockingMove;

            if (string.IsNullOrWhiteSpace(board[1, 1]))
                return new CellModel { Row = 1, Column = 1 };

            var corners = new List<CellModel>()
            {
                new CellModel { Row = 0, Column = 0 },
                new CellModel { Row = 0, Column = 2 },
                new CellModel { Row = 2, Column = 0 },
                new CellModel { Row = 2, Column = 2 }
            };

            var corner = corners.FirstOrDefault(c => string.IsNullOrWhiteSpace(board[c.Row, c.Column]));
            if (corner != null)
                return corner;

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    if (string.IsNullOrWhiteSpace(board[row, column]))
                        return new CellModel { Row = row, Column = column };
                }
            }

            throw new PZException(StatusCodes.Status400BadRequest, "No Computer Move Available", "");
        }

        private CellModel? FindBestMove(string[,] board, string player)
        {
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    if (string.IsNullOrWhiteSpace(board[row, column]))
                    {
                        board[row, column] = player;
                        var cells = GetWinningCells(board);
                        board[row, column] = "";

                        if (cells.Any())
                            return new CellModel { Row = row, Column = column };
                    }
                }
            }

            return null;
        }

        private string NextPlayer(string player)
        {
            return player == "X" ? "O" : "X";
        }

        private string SerializeCells(List<CellModel> cells)
        {
            return string.Join(";", cells.Select(c => $"{c.Row},{c.Column}"));
        }

        private List<CellModel> DeserializeCells(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<CellModel>();

            return value.Split(';')
                .Select(x => x.Split(','))
                .Where(x => x.Length == 2)
                .Select(x => new CellModel { Row = int.Parse(x[0]), Column = int.Parse(x[1]) })
                .ToList();
        }

        private async Task<GameStateResponse> BuildResponse(int gameId)
        {
            var game = await GetGameOrThrow(gameId);
            var scoreboard = await _gameRepository.GetScoreboard();
            var board = BuildBoard(game.Moves);
            var boardResponse = new List<List<string>>();

            for (var row = 0; row < 3; row++)
            {
                var rowValues = new List<string>();
                for (var column = 0; column < 3; column++)
                    rowValues.Add(board[row, column] ?? "");

                boardResponse.Add(rowValues);
            }

            return new GameStateResponse()
            {
                GameId = game.GameId,
                Board = boardResponse,
                CurrentPlayer = game.CurrentPlayer,
                GameMode = game.GameMode,
                GameStatus = game.GameStatus,
                Winner = game.Winner,
                WinningCells = DeserializeCells(game.WinningCells),
                MoveHistory = game.Moves.OrderBy(m => m.MoveNumber).Select(m => new MoveHistoryModel()
                {
                    MoveNumber = m.MoveNumber,
                    Player = m.Player,
                    Row = m.Row,
                    Column = m.Column,
                    Position = $"Row {m.Row + 1}, Column {m.Column + 1}"
                }).ToList(),
                CanUndo = game.GameStatus == "InProgress" && game.Moves.Any(),
                Scoreboard = MapScoreboard(scoreboard)
            };
        }

        private ScoreboardResponse MapScoreboard(Scoreboard scoreboard)
        {
            return new ScoreboardResponse()
            {
                XWins = scoreboard.XWins,
                OWins = scoreboard.OWins,
                Draws = scoreboard.Draws
            };
        }
        #endregion
    }
}
