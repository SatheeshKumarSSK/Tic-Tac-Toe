namespace TicTacToe.Api.Helpers
{
    public class PZExceptionResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public object Error { get; set; } = "";
        public string ErrorPath { get; set; } = "";
        public string Code { get; set; } = "";
        public string LanguageCode { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
