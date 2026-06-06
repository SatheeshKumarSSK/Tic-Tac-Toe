using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Data;
using TicTacToe.Api.Models.Entities;

namespace TicTacToe.Api.Repositories
{
    public interface IGameRepository
    {
        Task<GameSession> CreateGame(GameSession game);
        Task<GameSession?> GetGame(int gameId);
        Task<GameSession?> GetGameWithMoves(int gameId);
        Task<bool> AddMove(GameMove move);
        Task<bool> DeleteMoves(List<GameMove> moves);
        Task<bool> UpdateGame(GameSession game);
        Task<Scoreboard> GetScoreboard();
        Task<bool> UpdateScoreboard(Scoreboard scoreboard);
        Task<bool> ResetScoreboard();
    }

    public class GameRepository : IGameRepository
    {
        private readonly DataContext _context;

        public GameRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<GameSession> CreateGame(GameSession game)
        {
            await _context.GameSessions.AddAsync(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<GameSession?> GetGame(int gameId)
        {
            var game = await _context.GameSessions.FirstOrDefaultAsync(g => g.GameId == gameId && g.IsDeleted != true);
            return game;
        }

        public async Task<GameSession?> GetGameWithMoves(int gameId)
        {
            var game = await _context.GameSessions
                .Include(g => g.Moves)
                .FirstOrDefaultAsync(g => g.GameId == gameId && g.IsDeleted != true);
            return game;
        }

        public async Task<bool> AddMove(GameMove move)
        {
            await _context.GameMoves.AddAsync(move);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMoves(List<GameMove> moves)
        {
            _context.GameMoves.RemoveRange(moves);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateGame(GameSession game)
        {
            _context.GameSessions.Update(game);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Scoreboard> GetScoreboard()
        {
            var scoreboard = await _context.Scoreboards.FirstOrDefaultAsync();

            if (scoreboard == null)
            {
                scoreboard = new Scoreboard()
                {
                    XWins = 0,
                    OWins = 0,
                    Draws = 0,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                await _context.Scoreboards.AddAsync(scoreboard);
                await _context.SaveChangesAsync();
            }

            return scoreboard;
        }

        public async Task<bool> UpdateScoreboard(Scoreboard scoreboard)
        {
            scoreboard.DateModified = DateTime.UtcNow;
            _context.Scoreboards.Update(scoreboard);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetScoreboard()
        {
            var scoreboard = await GetScoreboard();
            scoreboard.XWins = 0;
            scoreboard.OWins = 0;
            scoreboard.Draws = 0;
            scoreboard.DateModified = DateTime.UtcNow;
            _context.Scoreboards.Update(scoreboard);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
