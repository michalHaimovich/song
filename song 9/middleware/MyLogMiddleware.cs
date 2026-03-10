using System.Diagnostics;
using System.Security.Claims;
using SongApi.interfaces; 

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate next;

    public MyLogMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext c, ILogQueueService logQueue)
    {
        var startTime = DateTime.Now;
        var sw = Stopwatch.StartNew();

        await next.Invoke(c);

        sw.Stop();
        var durationMs = sw.ElapsedMilliseconds;

        // מנסים לשלוף קונטרולר ופעולה
        var controllerName = c.Request.RouteValues["controller"]?.ToString();
        var actionName = c.Request.RouteValues["action"]?.ToString();

        if (string.IsNullOrEmpty(controllerName))
        {
            controllerName = "Non-Controller";
            actionName = c.Request.Path.ToString(); 
        }

        string userName = "Guest";
        if (c.User?.Identity?.IsAuthenticated == true)
        {
            userName = c.User.FindFirst("name")?.Value 
                       ?? c.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? "UnknownUser";
        }

        string logMessage = $"[{startTime:dd/MM/yyyy HH:mm:ss}] Target: {controllerName} | Action/Path: {actionName} | User: {userName} | Duration: {durationMs}ms\n";

        await logQueue.PublishLogAsync(logMessage);
    }
}
public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MyLogMiddleware>();
    }
}