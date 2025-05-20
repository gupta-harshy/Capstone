using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDependencies
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers HttpClientService and related dependencies in the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configureClient">Optional action to configure the HttpClient instance.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddHttpClientService(this IServiceCollection services, Action<HttpClient> configureClient = null)
        {
            // Register IHttpContextAccessor to access HTTP context in services
            services.AddHttpContextAccessor();

            // Register HttpClientService and configure HttpClient
            services.AddHttpClient<HttpClientService>(client =>
            {
                configureClient?.Invoke(client);
            });

            // If you need a scoped registration for the service (optional)
            services.AddScoped<HttpClientService>();

            return services;
        }
    }
}
