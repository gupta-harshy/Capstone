using Nest;

public class ElasticSearchService<T> : IElasticSearchService<T> where T : class
{
    private readonly IElasticClient _client;
    private readonly string _defaultIndex;

    public ElasticSearchService(IElasticClient client)
    {
        _client = client;
        _defaultIndex = typeof(T).Name.ToLower();
    }

    private string ResolveIndex(string? overrideIndex) =>
        !string.IsNullOrWhiteSpace(overrideIndex) ? overrideIndex.ToLower() : _defaultIndex;

    private async Task EnsureIndexExistsAsync(string index)
    {
        var existsResponse = await _client.Indices.ExistsAsync(index);

        if (!existsResponse.Exists)
        {
            var createResponse = await _client.Indices.CreateAsync(index, c =>
                c.Map<T>(m => m.AutoMap())
            );

            if (!createResponse.IsValid)
            {
                throw new Exception($"Failed to create index '{index}': {createResponse.ServerError?.Error.Reason}");
            }
        }
    }

    public async Task IndexAsync(T item, string? indexOverride = null)
    {
        var index = ResolveIndex(indexOverride);
        await EnsureIndexExistsAsync(index);

        var id = typeof(T).GetProperty("Id")?.GetValue(item)?.ToString();

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Item must have a valid 'Id' property to be used as Elasticsearch document ID.");

        await _client.IndexAsync(item, i => i
            .Index(index)
            .Id(id)
        );
    }

    public async Task<IEnumerable<T>> SearchAsync(string keyword, string[] fields, string? indexOverride = null)
    {
        var index = ResolveIndex(indexOverride);
        await EnsureIndexExistsAsync(index);

        var response = await _client.SearchAsync<T>(s => s
            .Index(index)
            .Query(q =>
                q.MultiMatch(m => m
                .Fields(f => f.Fields(fields)) // match on any of these fields
                .Query(keyword)
                .Type(TextQueryType.BestFields) // OR logic between fields (default)
                .Operator(Operator.Or)          // ensure "any field contains term" logic
                .Lenient(true)                  // ignore mapping mismatches
                )
            )
        );

        return response.Documents;
    }

    public async Task UpdateAsync(string id, T item, string? indexOverride = null)
    {
        var index = ResolveIndex(indexOverride);
        await EnsureIndexExistsAsync(index);
        await _client.UpdateAsync<T>(id, u => u.Index(index).Doc(item));
    }

    public async Task DeleteDocumentAsync(string id, string? indexOverride = null)
    {
        var index = ResolveIndex(indexOverride);
        await EnsureIndexExistsAsync(index);

        var response = await _client.DeleteAsync<T>(id, d => d.Index(index));

        if (!response.IsValid)
        {
            throw new Exception($"Failed to delete document with ID: {id} from index: {index}. Reason: {response.ServerError?.Error.Reason}");
        }
    }
}