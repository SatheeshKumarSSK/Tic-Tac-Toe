namespace TicTacToe.Api.Models.Game
{
    public class CreateGameRequest
    {
        public string GameMode { get; set; } = "";
    }

    public class MoveRequest
    {
        public int GameId { get; set; }
        public string Player { get; set; } = "";
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class CellModel
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class MoveHistoryModel
    {
        public int MoveNumber { get; set; }
        public string Player { get; set; } = "";
        public int Row { get; set; }
        public int Column { get; set; }
        public string Position { get; set; } = "";
    }

    public class ScoreboardResponse
    {
        public int XWins { get; set; }
        public int OWins { get; set; }
        public int Draws { get; set; }
    }

    public class GameStateResponse
    {
        public int GameId { get; set; }
        public List<List<string>> Board { get; set; } = new();
        public string CurrentPlayer { get; set; } = "";
        public string GameMode { get; set; } = "";
        public string GameStatus { get; set; } = "";
        public string? Winner { get; set; }
        public List<CellModel> WinningCells { get; set; } = new();
        public List<MoveHistoryModel> MoveHistory { get; set; } = new();
        public bool CanUndo { get; set; }
        public ScoreboardResponse Scoreboard { get; set; } = new();
    }
}
