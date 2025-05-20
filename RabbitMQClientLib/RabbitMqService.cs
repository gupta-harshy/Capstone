using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMqService : IRabbitMqPublisher, IRabbitMqConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService(IConfiguration configuration)
    {
        var config = configuration.GetSection("RabbitMq");
        var factory = new ConnectionFactory()
        {
            HostName = config["HostName"],
            Port = int.Parse(config["Port"] ?? "5672"),
            UserName = config["UserName"],
            Password = config["Password"]
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<T>(T message, string queueName)
    {
        DeclareQueue(queueName);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        return Task.CompletedTask;
    }

    public void Subscribe<T>(string queueName, Func<T, Task> onMessage)
    {
        DeclareQueue(queueName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
            int retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    if (message != null)
                    {
                        await onMessage(message);
                        return;
                    }
                }
                catch
                {
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            // Move to Dead-letter queue if retries exhausted
            var dlqName = $"{queueName}.dlq";
            _channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false);
            _channel.BasicPublish(exchange: "", routingKey: dlqName, body: body);
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    private void DeclareQueue(string queueName)
    {
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", $"{queueName}.dlq" }
        };

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
    }
}