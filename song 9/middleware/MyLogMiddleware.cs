using System.Diagnostics;

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;//


    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger)//ctor
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext c)
    {
        var sw = new Stopwatch();
        sw.Start();
        await next.Invoke(c);
        logger.LogDebug($"this call took this long: {sw} from log1 Middleware");
        
    }
}

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MyLogMiddleware>();
    }
}

