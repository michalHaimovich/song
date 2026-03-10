namespace SongApi.interfaces;
public interface ILogQueueService
{
    Task PublishLogAsync(string logMessage);
    IAsyncEnumerable<string> ReadAllLogsAsync(CancellationToken cancellationToken);
}