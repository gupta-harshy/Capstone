using Microsoft.Extensions.DependencyInjection;
using Nest;

public static class ElasticSearchExtensions
{
    public static IServiceCollection AddElasticSearch<T>(this IServiceCollection services, string uri)
        where T : class
    {
        var settings = new ConnectionSettings(new Uri(uri))
            .DefaultIndex(typeof(T).Name.ToLower());

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);
        services.AddSingleton<IElasticSearchService<T>, ElasticSearchService<T>>();

        return services;
    }
}