using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KsPizza.Hubs;
using KsPizza.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KsPizza.Services
{
    public class PizzaUpdateWorker : BackgroundService
    {
        private readonly IHubContext<ActivityHub> hubContext;
        private IConnection connection;
        private IChannel channel;
        private const string QueueName = "pizza-updates";

        public PizzaUpdateWorker(IHubContext<ActivityHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<PizzaUpdatedMessage>(json);

                // HEAVY OPERATIONS HAPPEN HERE (not in HTTP request thread!)
                Thread.Sleep(5000);  // Simulate invoice generation, analytics, etc.
                //await Task.Delay(5000);

                // Broadcast to SignalR after heavy work completes
                await hubContext.Clients.All.SendAsync(
                    "ReceiveActivity",
                    message.Username,
                    "updated",
                    message.PizzaName,
                    stoppingToken);

                // Acknowledge message
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,  // Manual acknowledgment for reliability
                consumer: consumer);

            // Keep the worker running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await channel?.CloseAsync();
            await connection?.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
