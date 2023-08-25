using ChatApi.Context;
using System.Text;


namespace ChatApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;

        }
        public async Task Invoke(HttpContext context, ChatDbContext _context)
        {
            var request = context.Request;
            var requestBody = await GetRequestBody(request);


            var log = new Model.Log
            {
                ipAddress = GetIpAddress(context),
                requestBody = requestBody,
                timeStamp = DateTime.Now,

            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

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

        private string GetIpAddress(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }

}
