using System.Diagnostics;
using System.Security.Claims;

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<MyLogMiddleware> logger;

    private static readonly object _fileLock = new object();

    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext c)
    {
        // 1. שמירת זמן ההתחלה והפעלת הסטופר
        var startTime = DateTime.Now;
        var sw = Stopwatch.StartNew();
        
        // מעבירים את הבקשה הלאה לקונטרולר שיעשה את העבודה שלו
        await next.Invoke(c);

        // עוצרים את הסטופר ברגע שהתשובה חוזרת
        sw.Stop();
        var durationMs = sw.ElapsedMilliseconds;

        var controllerName = c.Request.RouteValues["controller"]?.ToString() ?? "UnknownController";
        var actionName = c.Request.RouteValues["action"]?.ToString() ?? "UnknownAction";

        string userName = "Guest";
        if (c.User?.Identity?.IsAuthenticated == true)
        {
            userName = c.User.FindFirst("name")?.Value 
                       ?? c.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? "UnknownUser";
        }

        string logMessage = $"[{startTime:dd/MM/yyyy HH:mm:ss}] Controller: {controllerName} | Action: {actionName} | User: {userName} | Duration: {durationMs}ms\n";

        logger.LogInformation(logMessage);

        string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "requests_log.txt");
       lock (_fileLock)
        {
                File.AppendAllText(logFilePath, logMessage);
        }
    }
}

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MyLogMiddleware>();
    }
}