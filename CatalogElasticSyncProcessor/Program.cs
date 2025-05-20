namespace CatalogElasticSyncProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<CatalogSyncWorker>();
            builder.Services.AddHttpClient("CatalogService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["CatalogSync:CatalogBaseUrl"] ?? "") ; // container name in docker
            });

            builder.Services.AddRabbitMqClient();
            builder.Services.AddElasticSearch<CatalogIndexDto>(builder.Configuration["ElasticSearchUrl"]); // assuming you made it reusable
            var host = builder.Build();
            host.Run();
        }
    }
}