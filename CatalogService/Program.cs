
using Catalog.Application.Interfaces;
using CS =Catalog.Application.Services;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Domain.Entities;
using CatalogService.Catalog.Application.Services;
using CatalogService.Catalog.Application.Interfaces;
using CatalogService.Catalog.Application.DTOs;

namespace CatalogService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddElasticSearch<CatalogIndexDto>(builder.Configuration["ElasticSearchUrl"]);
            builder.Services.AddRabbitMqClient();
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                return new MongoClient(builder.Configuration.GetConnectionString("MongoDbSettings"));
            });
            builder.Services.AddHttpClient("InventoryService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["InventoryServiceUrl"]); // Docker service name
            });
            builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
            builder.Services.AddScoped<ICatalogService, CS.CatalogService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IMessagePublisher, CatalogEventPublisher>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
