using CheckoutOrchestrator.Activities;
using CheckoutOrchestrator.Workflows;
using CommonDependencies;
using Temporalio.Client;
using Temporalio.Worker;

namespace CheckoutOrchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register TemporalClient synchronously by blocking on the ConnectAsync
                    var configuration = context.Configuration;
                    services.AddSingleton<TemporalClient>(sp =>
                    {
                        var address = configuration["Temporal:Address"];
                        if (string.IsNullOrWhiteSpace(address))
                        {
                            throw new InvalidOperationException("Temporal:Address configuration is missing");
                        }

                        // ConnectAsync returns a Task<TemporalClient>
                        return TemporalClient.ConnectAsync(
                            new TemporalClientConnectOptions(address)
                        ).GetAwaiter().GetResult();
                    });
                    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
                    services.AddHttpClient("order", c => c.BaseAddress = new Uri(configuration["OrderService:BaseUrl"] ?? string.Empty));
                    services.AddHttpClient("inventory", c => c.BaseAddress = new Uri(configuration["InventoryService:BaseUrl"] ?? string.Empty));
                    services.AddHttpClient("auth", c => c.BaseAddress = new Uri(configuration["AuthService:BaseUrl"] ?? string.Empty));
                    services.AddHttpClient("cart", c => c.BaseAddress = new Uri(configuration["CartService:BaseUrl"] ?? string.Empty));
                    // Register your activities and workflow worker
                    services.AddSingleton<ICheckoutActivities, CheckoutActivities>();

                    services.AddSingleton<TemporalWorker>(sp =>
                    {
                        var client = sp.GetRequiredService<TemporalClient>();
                        var activities = sp.GetRequiredService<ICheckoutActivities>();

                        return new TemporalWorker(client, 
                            new TemporalWorkerOptions(taskQueue: "CHECKOUT_TASK_QUEUE")
                                    .AddWorkflow<CheckoutWorkflow>()
                                    .AddAllActivities(activities)
                            );
                    });

                    // Hosted service to start the worker
                    services.AddHostedService<Worker>();
                }).Build();
            host.Run();
        }
    }
}