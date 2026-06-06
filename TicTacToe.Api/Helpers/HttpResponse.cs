namespace TicTacToe.Api.Helpers
{
    public class HttpResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public HttpResponse(int status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}
