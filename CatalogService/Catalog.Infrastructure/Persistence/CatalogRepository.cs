using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;


namespace Catalog.Infrastructure.Persistence;

public class CatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<CatalogItem> _collection;

    public CatalogRepository(IConfiguration config, IMongoClient client)
    {
        var db = client.GetDatabase("CatalogDb");

        var collectionName = "CatalogItems";
        var collections = db.ListCollectionNames().ToList();

        if (!collections.Contains(collectionName))
        {
            db.CreateCollection(collectionName);
        }

        _collection = db.GetCollection<CatalogItem>(collectionName);
    }

    public async Task AddAsync(CatalogItem item) =>
        await _collection.InsertOneAsync(item);

    public async Task<CatalogItem?> GetByIdAsync(string id) =>
        await _collection.Find(i => i.Id == id.ToLower()).FirstOrDefaultAsync();

    public async Task UpdateAsync(CatalogItem item)
    {
        var update = Builders<CatalogItem>.Update
            .Set(x => x.Name, item.Name)
            .Set(x => x.Description, item.Description)
            .Set(x => x.Price, item.Price)
            .Set(x => x.Category, item.Category);

        await _collection.UpdateOneAsync(
            x => x.Id == item.Id.ToLower(),
            update);
    }

}