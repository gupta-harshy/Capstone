using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Catalog.Domain.Entities;

public class CatalogItem
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Category { get; private set; }

    public CatalogItem(string productId, string name, string description, decimal price, string category)
    {
        Id = productId;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
    }
}