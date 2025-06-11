using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Play.Catalog.Service
{
    public class ConsumeRabbitMQHostedServices : BackgroundService
    {
        private readonly ILogger<ConsumeRabbitMQHostedServices> _logger;
        private readonly Task _initializationTask;
        private IConnection _connection;
        private IChannel _channel;
        private object stoppingToken;

        public ConsumeRabbitMQHostedServices(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedServices>();
            _initializationTask = InitializeAsync(CancellationToken.None);
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await InitRabbitMQ();
                _logger.LogInformation("RabbitMQ connection initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ connection.");
                //throw new Exception(ex.Message);
            }
        }

        private async Task InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672/"), // Replace with your RabbitMQ connection string
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.BasicQosAsync(prefetchCount: 1, prefetchSize: 0, global: false);

            // Initialize RabbitMQ connection and channel here.
            // This method should set up the necessary configurations to connect to RabbitMQ.
            _logger.LogInformation("RabbitMQ initialized.");

            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
        }
        private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation("RabbitMQ connection shutdown: {Reason}", e.ReplyText);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _initializationTask;
            if(_channel == null || _connection == null)
            {
                _logger.LogError("RabbitMQ connection or channel is not initialized.");
                return;
            }
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($" [x] Received {message}");

                // Acknowledge the message
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            consumer.RegisteredAsync += async (model, ea) =>
            {
                _logger.LogInformation("Consumer registered: {ConsumerTag}", ea.ConsumerTags);
            };

            consumer.UnregisteredAsync += async (model, ea) =>
            {
                _logger.LogInformation("Consumer unregistered: {ConsumerTag}", ea.ConsumerTags);
            };

            await _channel.BasicConsumeAsync(queue: "task_queue", autoAck: false, consumer: consumer);
            _logger.LogInformation("Consumer started. Waiting for messages...");
        }

        public async override void Dispose()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            _logger.LogInformation("RabbitMQ connection disposed.");
            base.Dispose();
        }
    }
}
