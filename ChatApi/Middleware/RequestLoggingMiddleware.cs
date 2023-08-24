using ChatApi.Context;
using System.Text;


namespace ChatApi.Middleware
{
    public class RequestLoggingMiddleware
    {
            private readonly RequestDelegate _next;
            private readonly ILogger<RequestLoggingMiddleware> _logger;

            public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }
            public async Task Invoke(HttpContext context, ChatDbContext dbContext)
            {
                var request = context.Request;
                var requestBody = await GetRequestBody(request);


                var log = new Model.Log
                {
                    ipAddress = GetIpAddress(context),
                    requestBody = requestBody,
                    timeStamp = DateTime.Now,
                
                };

                dbContext.Logs.Add(log);
                await dbContext.SaveChangesAsync();

                await _next(context);
            }

            private async Task<string> GetRequestBody(HttpRequest request)
            {
                request.EnableBuffering();
                var reqBody = string.Empty;

                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    reqBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }

                return reqBody;
            }

            private string GetIpAddress(HttpContext context)
            {
                return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            }



        }

        // Extension 
        public static class RequestLoggingMiddlewareExtensions
        {
            public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<RequestLoggingMiddleware>();
            }
        }

    }
