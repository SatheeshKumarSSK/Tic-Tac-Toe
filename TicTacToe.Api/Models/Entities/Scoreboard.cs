using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.Api.Models.Entities
{
    [Table("Scoreboards")]
    public class Scoreboard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScoreboardId { get; set; }
        public int XWins { get; set; }
        public int OWins { get; set; }
        public int Draws { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
