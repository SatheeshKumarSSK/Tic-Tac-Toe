namespace TicTacToe.Api.Helpers
{
    public class PZException : Exception
    {
        public int Status { get; set; }
        public object Error { get; set; }
        public string ErrorPath { get; set; }
        public string Code { get; set; }
        public string LanguageCode { get; set; }
        public DateTime Timestamp { get; set; }

        public PZException(string message) : base(message)
        {
            Status = StatusCodes.Status400BadRequest;
            Error = "";
            ErrorPath = "";
            Code = "";
            LanguageCode = "";
            Timestamp = DateTime.UtcNow;
        }

        public PZException(int status, string message, object error) : base(message)
        {
            Status = status;
            Error = error;
            ErrorPath = "";
            Code = "";
            LanguageCode = "";
            Timestamp = DateTime.UtcNow;
        }

        public PZException(string message, object error, int status) : base(message)
        {
            Status = status;
            Error = error;
            ErrorPath = "";
            Code = "";
            LanguageCode = "";
            Timestamp = DateTime.UtcNow;
        }
    }
}
