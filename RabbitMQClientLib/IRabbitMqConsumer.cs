public interface IRabbitMqConsumer
{
    void Subscribe<T>(string queueName, Func<T, Task> onMessage);
}