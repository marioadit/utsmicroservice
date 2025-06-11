using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Play.Transaction.Service.Controllers
{
    // RabbitMqPublisher is a service that publishes messages to a RabbitMQ queue.
    // It uses the RabbitMQ.Client library to connect to RabbitMQ and send messages.
    // The messages are serialized to JSON format before being sent.
    // The queue is durable, meaning messages will be saved even if RabbitMQ restarts.
    // The publisher logs information about the message being published.

    public class RabbitMqPublisher
    {
        private readonly ConnectionFactory factory;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private const string QueueName = "task_queue";

        public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;
            var uri = configuration["RabbitMQ:Uri"];
            factory = new ConnectionFactory()
            {
                Uri = new Uri(uri ?? "amqp://guest:guest@localhost:5672")
            };
        }

        public async Task PublishMessageAsync(object message)
        {
            _logger.LogInformation("Preparing to publish to RabbitMQ: {@Message}", message);

            using var _connection = await factory.CreateConnectionAsync();
            using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                DeliveryMode = (DeliveryModes)2 // Make message persistent
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Message successfully published to queue {QueueName}", QueueName);
        }


    }
}

// to use the RabbitMqPublisher in the SalesItemController, you would inject it in the constructor and call it like this:
// var stockChangeMessage = new
// {
//     ProductId = existing.ProductId,
//     QuantityChanged = -existing.Quantity,
//     Action = "StockDecreased",
//     Source = "SalesItemController.Create",
//     Timestamp = DateTimeOffset.UtcNow
// };

// await rabbitMqPublisher.PublishMessageAsync(stockChangeMessage);