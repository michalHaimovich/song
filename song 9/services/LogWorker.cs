using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SongApi.interfaces;

namespace SongApi.Services;

public class LogWorker : BackgroundService
{
    private readonly ILogQueueService _logQueue;
    private readonly object _fileLock = new object();

    public LogWorker(ILogQueueService logQueue)
    {
        _logQueue = logQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var logMessage in _logQueue.ReadAllLogsAsync(stoppingToken))
        {
            // === זיוף הפעולה הכבדה ===
            await Task.Delay(5000, stoppingToken);

            // יצירת תיקייה וקובץ לפי תאריך
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var fileName = $"{DateTime.Now:yyyy-MM-dd}.txt";
            var fullPath = Path.Combine(logDirectory, fileName);

            // כתיבה בטוחה לקובץ
            lock (_fileLock)
            {
                File.AppendAllText(fullPath, logMessage);
            }
        }
    }
}