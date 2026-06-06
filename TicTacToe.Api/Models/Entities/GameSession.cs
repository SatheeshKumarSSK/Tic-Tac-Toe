using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.Api.Models.Entities
{
    [Table("GameSessions")]
    public class GameSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }
        public string GameMode { get; set; } = "";
        public string CurrentPlayer { get; set; } = "";
        public string GameStatus { get; set; } = "";
        public string? Winner { get; set; }
        public string? WinningCells { get; set; }
        public bool ScoreboardUpdated { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual List<GameMove> Moves { get; set; } = new();
    }
}
