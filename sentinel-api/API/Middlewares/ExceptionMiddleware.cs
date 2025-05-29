using sentinel_api.Application.Common;
using System.Net;

namespace sentinel_api.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu uma exceção não tratada.");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                string userMessage = "Ocorreu um erro inesperado. Tente novamente mais tarde.";

                if (_env.IsDevelopment())
                {
                    var resultDev = Result.Failure($"{ex.Message}\n{ex.StackTrace}");
                    await context.Response.WriteAsJsonAsync(resultDev);
                }
                else
                {
                    var resultProd = Result.Failure(userMessage);
                    await context.Response.WriteAsJsonAsync(resultProd);
                }
            }
        }
    }
}
