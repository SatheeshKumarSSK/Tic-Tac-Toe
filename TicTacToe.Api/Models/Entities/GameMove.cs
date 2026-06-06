using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.Api.Models.Entities
{
    [Table("GameMoves")]
    public class GameMove
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameMoveId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }
        public string Player { get; set; } = "";
        public int Row { get; set; }
        public int Column { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual GameSession GameSession { get; set; } = null!;
    }
}
