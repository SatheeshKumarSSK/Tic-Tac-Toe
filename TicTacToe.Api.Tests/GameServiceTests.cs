using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicTacToe.Api.Data;
using TicTacToe.Api.Helpers;
using TicTacToe.Api.Models.Entities;
using TicTacToe.Api.Models.Game;
using TicTacToe.Api.Repositories;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests
{
    public class GameServiceTests
    {
        [Fact]
        public async Task ValidMove_ShouldUpdateBoardAndSwitchTurn()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));

            Assert.Equal("X", result.Board[0][0]);
            Assert.Equal("O", result.CurrentPlayer);
            Assert.Single(result.MoveHistory);
        }

        [Fact]
        public async Task MoveOutsideTheBoard_ShouldRejectMove()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await Assert.ThrowsAsync<PZException>(() => service.MakeMove(game.GameId, Move(game.GameId, "X", 3, 0)));
            var result = await service.GetGame(game.GameId);

            Assert.Equal("X", result.CurrentPlayer);
            Assert.Empty(result.MoveHistory);
        }

        [Fact]
        public async Task MoveOnOccupiedCell_ShouldRejectMove()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));

            await Assert.ThrowsAsync<PZException>(() => service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 0)));
            var result = await service.GetGame(game.GameId);

            Assert.Equal("O", result.CurrentPlayer);
            Assert.Single(result.MoveHistory);
        }

        [Fact]
        public async Task MoveAfterGameCompletion_ShouldRejectMove()
        {
            var (service, _) = CreateService();
            var game = await CreateXRowWin(service);

            await Assert.ThrowsAsync<PZException>(() => service.MakeMove(game.GameId, Move(game.GameId, "O", 2, 2)));
        }

        [Fact]
        public async Task MoveByWrongPlayer_ShouldRejectMove()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await Assert.ThrowsAsync<PZException>(() => service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 0)));
            var result = await service.GetGame(game.GameId);

            Assert.Equal("X", result.CurrentPlayer);
            Assert.Empty(result.MoveHistory);
        }

        [Fact]
        public async Task InvalidMove_ShouldNotChangeTurn()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await Assert.ThrowsAsync<PZException>(() => service.MakeMove(game.GameId, Move(game.GameId, "X", -1, 0)));
            var result = await service.GetGame(game.GameId);

            Assert.Equal("X", result.CurrentPlayer);
        }

        [Fact]
        public async Task TurnSwitching_ShouldAlternatePlayers()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            var afterX = await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            var afterO = await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));

            Assert.Equal("O", afterX.CurrentPlayer);
            Assert.Equal("X", afterO.CurrentPlayer);
        }

        [Fact]
        public async Task RowWin_ShouldCompleteGame()
        {
            var (service, _) = CreateService();
            var result = await CreateXRowWin(service);

            Assert.Equal("Won", result.GameStatus);
            Assert.Equal("X", result.Winner);
            Assert.Equal(3, result.WinningCells.Count);
        }

        [Fact]
        public async Task ColumnWin_ShouldCompleteGame()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 1, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));
            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 2, 0));

            Assert.Equal("Won", result.GameStatus);
            Assert.Equal("X", result.Winner);
            Assert.All(result.WinningCells, c => Assert.Equal(0, c.Column));
        }

        [Fact]
        public async Task DiagonalWin_ShouldCompleteGame()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 1, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 2));
            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 2, 2));

            Assert.Equal("Won", result.GameStatus);
            Assert.Equal("X", result.Winner);
            Assert.Contains(result.WinningCells, c => c.Row == 1 && c.Column == 1);
        }

        [Fact]
        public async Task Draw_ShouldCompleteGame()
        {
            var (service, _) = CreateService();
            var result = await CreateDraw(service);

            Assert.Equal("Draw", result.GameStatus);
            Assert.Null(result.Winner);
            Assert.Equal(9, result.MoveHistory.Count);
        }

        [Fact]
        public async Task WinningCells_ShouldReturnWinningLine()
        {
            var (service, _) = CreateService();
            var result = await CreateXRowWin(service);

            Assert.Contains(result.WinningCells, c => c.Row == 0 && c.Column == 0);
            Assert.Contains(result.WinningCells, c => c.Row == 0 && c.Column == 1);
            Assert.Contains(result.WinningCells, c => c.Row == 0 && c.Column == 2);
        }

        [Fact]
        public async Task ResetGame_ShouldClearBoardAndKeepScoreboard()
        {
            var (service, _) = CreateService();
            var game = await CreateXRowWin(service);

            var result = await service.ResetGame(game.GameId);

            Assert.Equal("InProgress", result.GameStatus);
            Assert.Equal("X", result.CurrentPlayer);
            Assert.Empty(result.MoveHistory);
            Assert.Equal(1, result.Scoreboard.XWins);
        }

        [Fact]
        public async Task UndoInTwoPlayerMode_ShouldRemoveLatestMove()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));
            var result = await service.Undo(game.GameId);

            Assert.Single(result.MoveHistory);
            Assert.Equal("O", result.CurrentPlayer);
            Assert.Equal("", result.Board[1][1]);
        }

        [Fact]
        public async Task UndoInComputerMode_ShouldRemoveComputerAndHumanPair()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "Computer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            var result = await service.Undo(game.GameId);

            Assert.Empty(result.MoveHistory);
            Assert.Equal("X", result.CurrentPlayer);
            Assert.All(result.Board.SelectMany(r => r), cell => Assert.Equal("", cell));
        }

        [Fact]
        public async Task UndoAfterCompletion_ShouldBeDisabled()
        {
            var (service, _) = CreateService();
            var game = await CreateXRowWin(service);

            await Assert.ThrowsAsync<PZException>(() => service.Undo(game.GameId));
        }

        [Fact]
        public async Task Scoreboard_ShouldUpdateOnXWin()
        {
            var (service, _) = CreateService();
            var result = await CreateXRowWin(service);

            Assert.Equal(1, result.Scoreboard.XWins);
            Assert.Equal(0, result.Scoreboard.OWins);
        }

        [Fact]
        public async Task Scoreboard_ShouldUpdateOnOWin()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });

            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 1, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 2, 2));
            var result = await service.MakeMove(game.GameId, Move(game.GameId, "O", 2, 1));

            Assert.Equal("O", result.Winner);
            Assert.Equal(1, result.Scoreboard.OWins);
        }

        [Fact]
        public async Task Scoreboard_ShouldUpdateOnDraw()
        {
            var (service, _) = CreateService();
            var result = await CreateDraw(service);

            Assert.Equal(1, result.Scoreboard.Draws);
        }

        [Fact]
        public async Task Scoreboard_ShouldUpdateOnlyOnceForCompletedGame()
        {
            var (service, _) = CreateService();
            var game = await CreateXRowWin(service);

            await service.GetGame(game.GameId);
            await service.GetGame(game.GameId);
            var scoreboard = await service.GetScoreboard();

            Assert.Equal(1, scoreboard.XWins);
        }

        [Fact]
        public async Task ResetScoreboard_ShouldClearCounts()
        {
            var (service, _) = CreateService();
            await CreateXRowWin(service);

            var result = await service.ResetScoreboard();

            Assert.Equal(0, result.XWins);
            Assert.Equal(0, result.OWins);
            Assert.Equal(0, result.Draws);
        }

        [Fact]
        public async Task ComputerMove_ShouldPlayWinningMove()
        {
            var (service, context) = CreateService();
            var gameId = SeedComputerGame(context,
            [
                MoveSeed("O", 0, 0, 1),
                MoveSeed("O", 0, 1, 2),
                MoveSeed("X", 1, 1, 3)
            ]);

            var result = await service.MakeMove(gameId, Move(gameId, "X", 2, 2));

            Assert.Equal("Won", result.GameStatus);
            Assert.Equal("O", result.Winner);
            Assert.Equal("O", result.Board[0][2]);
        }

        [Fact]
        public async Task ComputerMove_ShouldBlockHumanWinningMove()
        {
            var (service, context) = CreateService();
            var gameId = SeedComputerGame(context,
            [
                MoveSeed("X", 0, 0, 1),
                MoveSeed("X", 0, 1, 2),
                MoveSeed("O", 1, 1, 3)
            ]);

            var result = await service.MakeMove(gameId, Move(gameId, "X", 2, 2));

            Assert.Equal("O", result.Board[0][2]);
            Assert.Equal("InProgress", result.GameStatus);
        }

        [Fact]
        public async Task ComputerMove_ShouldTakeCenter()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "Computer" });

            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));

            Assert.Equal("O", result.Board[1][1]);
        }

        [Fact]
        public async Task ComputerMove_ShouldTakeCornerFallback()
        {
            var (service, _) = CreateService();
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "Computer" });

            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 1, 1));

            Assert.Equal("O", result.Board[0][0]);
        }

        [Fact]
        public async Task ComputerMove_ShouldTakeAnyAvailableCellFallback()
        {
            var (service, context) = CreateService();
            var gameId = SeedComputerGame(context,
            [
                MoveSeed("X", 0, 0, 1),
                MoveSeed("O", 0, 1, 2),
                MoveSeed("O", 0, 2, 3),
                MoveSeed("X", 1, 1, 4),
                MoveSeed("X", 2, 0, 5),
                MoveSeed("O", 2, 2, 6)
            ]);

            var result = await service.MakeMove(gameId, Move(gameId, "X", 1, 2));

            Assert.Equal("O", result.Board[1][0]);
        }

        private static (GameService service, DataContext context) CreateService()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "ConnectionStrings:DefaultConnection", "Data Source=:memory:" }
                })
                .Build();

            var context = new DataContext(options, configuration);
            context.Scoreboards.Add(new Scoreboard()
            {
                XWins = 0,
                OWins = 0,
                Draws = 0,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = new GameRepository(context);
            var service = new GameService(repository);

            return (service, context);
        }

        private static MoveRequest Move(int gameId, string player, int row, int column)
        {
            return new MoveRequest()
            {
                GameId = gameId,
                Player = player,
                Row = row,
                Column = column
            };
        }

        private static GameMove MoveSeed(string player, int row, int column, int moveNumber)
        {
            return new GameMove()
            {
                Player = player,
                Row = row,
                Column = column,
                MoveNumber = moveNumber,
                DateCreated = DateTime.UtcNow
            };
        }

        private static int SeedComputerGame(DataContext context, List<GameMove> moves)
        {
            var game = new GameSession()
            {
                GameMode = "Computer",
                CurrentPlayer = "X",
                GameStatus = "InProgress",
                Winner = null,
                WinningCells = "",
                ScoreboardUpdated = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                Moves = moves
            };

            context.GameSessions.Add(game);
            context.SaveChanges();

            return game.GameId;
        }

        private static async Task<GameStateResponse> CreateXRowWin(GameService service)
        {
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));
            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 2));

            return result;
        }

        private static async Task<GameStateResponse> CreateDraw(GameService service)
        {
            var game = await service.CreateGame(new CreateGameRequest { GameMode = "TwoPlayer" });
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 0, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 0, 2));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 1, 0));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 1, 2));
            await service.MakeMove(game.GameId, Move(game.GameId, "X", 2, 1));
            await service.MakeMove(game.GameId, Move(game.GameId, "O", 2, 0));
            var result = await service.MakeMove(game.GameId, Move(game.GameId, "X", 2, 2));

            return result;
        }
    }
}
