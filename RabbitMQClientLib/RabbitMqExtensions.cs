using Microsoft.Extensions.DependencyInjection;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMqClient(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqPublisher, RabbitMqService>();
        services.AddSingleton<IRabbitMqConsumer, RabbitMqService>();
        return services;
    }
}