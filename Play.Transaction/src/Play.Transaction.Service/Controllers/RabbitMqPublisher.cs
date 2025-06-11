using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Play.Transaction.Service.Controllers
{
    public class RabbitMqPublisher
    {
        private readonly ConnectionFactory factory;
        private const string QueueName = "task_queue";

        public RabbitMqPublisher(IConfiguration configuration)
        {
            var uri = configuration["RabbitMQ:Uri"];
            factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
        }

        public async Task PublishMessageAsync(object message)
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

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
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body);
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