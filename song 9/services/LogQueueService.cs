using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SongApi.interfaces;

namespace SongApi.Services;



public class LogQueueService : ILogQueueService
{
    private readonly Channel<string> _channel;

    public LogQueueService()
    {
        _channel = Channel.CreateUnbounded<string>();
    }

    public async Task PublishLogAsync(string logMessage)
    {
        await _channel.Writer.WriteAsync(logMessage);
    }

    public IAsyncEnumerable<string> ReadAllLogsAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}