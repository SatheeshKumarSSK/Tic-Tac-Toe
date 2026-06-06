using Microsoft.EntityFrameworkCore;
using System.Data;
using TicTacToe.Api.Models.Entities;

namespace TicTacToe.Api.Data
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tictactoe.db";
        }

        public virtual DbSet<GameSession> GameSessions { get; set; }
        public virtual DbSet<GameMove> GameMoves { get; set; }
        public virtual DbSet<Scoreboard> Scoreboards { get; set; }

        public IDbConnection CreateConnection()
            => new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameSession>()
                .HasMany(g => g.Moves)
                .WithOne(m => m.GameSession)
                .HasForeignKey(m => m.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
