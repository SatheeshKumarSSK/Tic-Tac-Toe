using System.Net;
using TicTacToe.Api.Helpers;

namespace TicTacToe.Api.Middlewares
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (PZException e)
            {
                _logger.LogError(e.ToString());
                context.Response.StatusCode = e.Status == 0 ? StatusCodes.Status400BadRequest : e.Status;
                context.Response.ContentType = "application/json";

                var response = new PZExceptionResponse()
                {
                    Status = context.Response.StatusCode,
                    Message = e.Message,
                    Error = e.Error,
                    ErrorPath = e.ErrorPath,
                    Code = e.Code,
                    LanguageCode = e.LanguageCode,
                    Timestamp = e.Timestamp
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                context.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                context.Response.ContentType = "application/json";

                var response = new PZExceptionResponse()
                {
                    Status = context.Response.StatusCode,
                    Message = "An unexpected error occured",
                    Error = e.Message,
                    ErrorPath = "",
                    Code = "",
                    LanguageCode = "",
                    Timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
