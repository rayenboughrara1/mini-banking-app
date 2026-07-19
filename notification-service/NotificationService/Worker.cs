using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NotificationService v3 starting, attempting RabbitMQ connection...");

        var factory = new ConnectionFactory { HostName = "rabbitmq-service" };
        IConnection? connection = null;
        int attempt = 0;

        while (connection is null && !stoppingToken.IsCancellationRequested)
        {
            attempt++;
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                logger.LogInformation("Connected to RabbitMQ on attempt {attempt}", attempt);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Attempt {attempt} to connect to RabbitMQ failed: {message}. Retrying in 5 seconds...", attempt, ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection is null) return;

        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "transactions-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        logger.LogInformation("NotificationService is listening on 'transactions-queue'...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            logger.LogInformation("Notification received: {message}", message);
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: "transactions-queue",
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}